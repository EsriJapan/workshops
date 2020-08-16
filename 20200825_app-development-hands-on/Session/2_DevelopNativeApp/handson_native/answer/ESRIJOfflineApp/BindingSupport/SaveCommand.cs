using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ESRIJOfflineApp.BindingSupport
{
    public class SaveCommand : ICommand
    {
        //Command実行時に実行するアクション、引数を受け取りたい場合はこのActionをAction<object>などにする
        private Action _action;

        public SaveCommand(Action action)
        {//コンストラクタでActionを登録
            _action = action;
        }

        #region ICommandインターフェースの必須実装

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {//とりあえずActionがあれば実行可能
            return _action != null;
        }

        public void Execute(object parameter)
        {//今回は引数を使わずActionを実行
            _action?.Invoke();
        }

        #endregion
    }
}
