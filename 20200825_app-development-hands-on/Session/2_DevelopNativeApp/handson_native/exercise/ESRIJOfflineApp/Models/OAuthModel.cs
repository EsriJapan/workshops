using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ESRIJOfflineApp.Models
{
    public class OAuthModel
    {
        #region App.config の値を取得
        // - 認証するポータルの URL
        private string _serverUrl = ConfigurationManager.AppSettings["ServerUrl"];
        // - サーバに登録したクライアント ID.
        private string AppClientId = ConfigurationManager.AppSettings["AppClientId"];
        // - アプリケーションの秘密キー
        private string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        // - リダイレクトされる URL.
        private string OAuthRedirectUrl = ConfigurationManager.AppSettings["OAuthRedirectUrl"];
        // - サーバにホスティングしている Web マップアイテムの ID.
        private string WebMapId = ConfigurationManager.AppSettings["WebMapId"];
        // - ダウンロードしたオフラインデータの保存先.
        private string _offlineDataFolder = ConfigurationManager.AppSettings["OfflineDataFolder"];
        // - アプリケーションのモード（Online or Offline）.
        private string _mode = ConfigurationManager.AppSettings["Mode"];
        // - 最後に表示していたオフラインデータの名前.
        private string _offlineDataName = ConfigurationManager.AppSettings["OfflineDataName"];
        #endregion

        public void SetOAuthInfo()
        {
            // OAuthの設定を含むサーバー情報をAuthenticationManagerに登録します。
            ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(_serverUrl),
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                }
            };

            // ClientSecretを使用する場合、認証を OAuthAuthorizationCode で行います。
            if (!String.IsNullOrEmpty(ClientSecret))
            {
                // Use OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret).
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
                serverInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }

            // ArcGIS Online のサーバ情報を AuthenticationManager に登録する。
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // OAuth 通信を処理するには、カスタム OAuthAuthorize クラス (このモジュールで定義されています) を使用します。
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // このクラスのメソッドを使用して資格情報の取得にチャレンジする新しい ChallengeHandler を作成します。
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {

            // 認証情報が既に設定されている場合は、設定された値を返します。
            foreach (var cred in AuthenticationManager.Current.Credentials)
            {
                if (cred.ServiceUri == new Uri(_serverUrl))
                {
                    return cred;
                }
            }

            // セキュリティで保護されたリソースにアクセスした際に呼び出されるAuthenticationManagerのChallengeHandler関数です。
            Credential credential = null;

            try
            {
                // AuthenticationManager は、認証するためにユーザーに認証情報の入力を求めます。
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception e)
            {
                // Exception will be reported in calling function.
                Debug.WriteLine(e);
                throw;
            }

            return credential;
        }
    }
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // OAuth UI を格納するウィンドウ。
        private Window _authWindow;

        // 認可の完了を追跡する TaskCompletionSource。
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // 認可コールバック結果の URL (アプリケーションに構成されたリダイレクト URI)。
        private string _callbackUrl;

        // OAuth リクエストを処理する URL。
        private string _authorizeUrl;

        // 認可要求を処理するための関数。これは、セキュアなサービス、認可エンドポイント、リダイレクト URI の URI を受け取ります。
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // TaskCompletionSource.Taskが完了していない場合、承認は進行中です。
            if (_taskCompletionSource != null || _authWindow != null)
            {
                // 一度に1つの認可プロセスだけを許可します。
                throw new Exception("Authorization is in progress");
            }

            // 認可とリダイレクトのURLを格納します。
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // タスク完了ソースを作成して完了を追跡します。
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // ログインコントロールを表示する関数を呼び出し、UIスレッド上で実行されていることを確認してください。
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // TaskCompletionSourceに関連付けられたタスクを返します。
            return _taskCompletionSource.Task;
        }

        // UI スレッド上で OAuth 資格情報をチャレンジする機能。
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // 認証ページを表示するためのWebBrowserコントロールを作成します。
            WebBrowser authBrowser = new WebBrowser();

            // リダイレクトURLに送信されたレスポンスを確認するためのブラウザのナビゲートイベントを処理します。
            authBrowser.Navigating += WebBrowserOnNavigating;

            // ウェブブラウザを新しいウィンドウで表示します。
            _authWindow = new Window
            {
                Content = authBrowser,
                Height = 420,
                Width = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // アプリのウィンドウをブラウザのウィンドウの所有者に設定します（メインウィンドウが閉じればブラウザも閉じます）。
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _authWindow.Owner = Application.Current.MainWindow;
            }

            // ウィンドウが閉じたイベントを処理してから、認証URLに移動します。
            _authWindow.Closed += OnWindowClosed;
            authBrowser.Navigate(authorizeUri);

            // ウィンドウを表示します。
            if (_authWindow != null)
            {
                _authWindow.ShowDialog();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            // ブラウザのウィンドウが閉じたら、フォーカスをメインウィンドウに戻します。
            if (_authWindow != null && _authWindow.Owner != null)
            {
                _authWindow.Owner.Focus();
            }

            // タスクが完了していない場合は、ログインせずにウィンドウを閉じている必要があります。
            if (_taskCompletionSource != null && !_taskCompletionSource.Task.IsCompleted)
            {
                // キャンセルされた操作を示すタスク完了を設定します。
                _taskCompletionSource.TrySetCanceled();
            }

            _taskCompletionSource = null;
            _authWindow = null;
        }

        // ブラウザのナビゲーション（ページ内容の変更）を処理します。
        private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // コールバックURLへの応答を確認します。
            WebBrowser webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // ブラウザがない場合、URIがない場合、または空のURLが返されます。
            if (webBrowser == null || uri == null || _taskCompletionSource == null || String.IsNullOrEmpty(uri.AbsoluteUri))
            {
                return;
            }

            // 新しいコンテンツがコールバックURLからのものかどうかを確認します。
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl);

            if (isRedirected)
            {
                // イベントをキャンセルして、他の場所で処理されないようにします。
                e.Cancel = true;

                // タスク完了ソースのローカルコピーを取得します。
                TaskCompletionSource<IDictionary<string, string>> tcs = _taskCompletionSource;
                _taskCompletionSource = null;

                // ウィンドウを閉じます。
                if (_authWindow != null)
                {
                    _authWindow.Close();
                }

                // ヘルパー関数を呼び出して、応答パラメータ(認証キーを含む)をデコードします。
                IDictionary<string, string> authResponse = DecodeParameters(uri);

                // タスク完了ソースの結果を設定します。
                tcs.SetResult(authResponse);
            }
        }

        // querystringの値をキーと値の dictionary にデコードするヘルパー関数。
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // OAuth 認可応答 URI クエリ文字列で返されるキー値ペアの dictionary を作成します。
            string answer = "";

            // URIフラグメントまたはクエリ文字列から値を取得します。
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // パラメータをキー/値のペアに解析します。
            Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // 文字列のキー/値の dictionary を返します。
            return keyValueDictionary;
        }
    }
}
