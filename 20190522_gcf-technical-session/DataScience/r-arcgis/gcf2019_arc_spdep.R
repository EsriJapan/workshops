# 必要に応じてパッケージをインストール=========================================================
# install.packages("spData", dependencies = TRUE)
# install.packages("sf", dependencies = TRUE)
# install.packages("tidyverse", dependencies = TRUE)
# install.packages("spdep", dependencies = TRUE)
# install.packages("spatialreg", dependencies = TRUE)
# install.packages("ggthemes", dependencies = TRUE)


# spDataパッケージのボストンの住宅価格のデータを ArcGIS へ渡す=================================
require(sf)
bstn_shp <- read_sf(system.file("shapes/boston_tracts.shp", package = "spData"))
head(bstn_shp)

# arcgisbindingをインポート
require(arcgisbinding)
arc.check_product()

# プロジェクトのFGBDへ書き込む
arc.write(path = "gcf2019_sem/gcf2019_sem.gdb/bstn_house_price", data = bstn_shp)

# ArcGIS Proのデータを読み込む===============================================================
d <- arc.open("gcf2019_sem/gcf2019_sem.gdb/bstn_house_price")
bstn_fc <- arc.select(d, c("OBJECTID", "Shape", "CMEDV", "CRIM", "NOX", "RM", "DIS", "LSTAT", "BB", "POP"))
head(bstn_fc)

# Rのspdepパッケージを使って空間誤差モデルの解析をする=======================================
bstn_sp <- arc.data2sp(bstn_fc)

require(spdep)
W <- poly2nb(bstn_sp, queen = TRUE) %>% 
    nb2listw(style = "W", zero.policy = TRUE)

require(spatialreg)
SEM <- errorsarlm(
    formula = CMEDV ~ CRIM + NOX + RM + DIS + LSTAT + BB + POP,
    listw = W,
    data = bstn_sp
)
summary(SEM)

# 解析結果を元のデータに付与する=================================================================
# std_residual <- (residuals(SEM) - mean(residuals(SEM)))/3.8285
sem_values <- bstn_sp@data %>% 
    cbind(residuals = SEM$residuals) %>% 
    cbind(fitted = SEM$fitted.values)
bstn_sp@data <- (sem_values)
head(bstn_sp@data)

# 結果の可視化===============================================================================
require(ggplot2)
require(ggthemes)
g <- ggplot(bstn_sp@data, aes(x = fitted, y = residuals, ymin = -30, ymax = 30, xmin = -10, xmax = 50))
g <- g + theme_hc(base_size = 16)
g <- g + coord_fixed(ratio=1)
g <- g + geom_point(colour = "#003366", alpha = 0.4)
g <- g + geom_abline(aes(slope=0, intercept=0), color='red', linetype = "31")
g <- g + labs(x = "Predicted", y = "Residuals")
g

g <- ggplot(bstn_sp@data, aes(x = CMEDV, y = fitted, ymin = 0, ymax = 55, xmin = 0, xmax = 55))
g <- g + theme_hc(base_size = 16)
g <- g + coord_fixed(ratio=1)
g <- g + geom_point(colour = "#003366", alpha = 0.4)
g <- g + geom_abline(aes(slope=1, intercept=0), color='red', linetype = "31")
g <- g + labs(x = "Observed", y = "Predicted")
g

# データをArcGIS Proへ戻す==================================================================
arc.write(bstn_sp, path = "gcf2019_sem/gcf2019_sem.gdb/sem_rslt", overwrite = TRUE)
