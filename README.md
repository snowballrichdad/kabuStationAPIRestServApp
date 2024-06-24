
# C# Sample Web Application - README

## 概要
このプロジェクトは、ASP.NET Coreを使用して構築されたサンプルWebアプリケーションです。このアプリケーションは、kabuステーションAPIと連携し、売買注文の処理を行う機能を提供します。Serilogを使用したロギング、HTTPSの設定、及びエントリとエグジットの処理が含まれています。

## 特徴
- HTTPS設定
- Serilogを使用した詳細なロギング
- kabuステーションAPIとの連携
- 売買注文のエントリとエグジット処理

## 動作環境
- .NET 5.0以上
- kabuステーションAPI

## インストール
1. リポジトリをクローンします。
    ```bash
    git clone https://github.com/snowballrichdad/kabuStationAPIRestServApp.git
    ```
2. プロジェクトディレクトリに移動します。
    ```bash
    cd kabuStationAPIRestServApp/RestSrvApp
    ```
3. 必要なパッケージをインストールします。
    ```bash
    dotnet restore
    ```

## 設定
1. `appsettings.json`ファイルを編集して、kabuステーションAPIの設定を行います。
    ```json
    {
      "KabuApi": {
        "BaseUrl": "http://localhost:18080/kabusapi",
        "Token": "YOUR_API_TOKEN"
      },
      "Serilog": {
        "MinimumLevel": "Information",
        "WriteTo": [
          {
            "Name": "Console"
          }
        ]
      }
    }
    ```

## 使い方
アプリケーションを起動します。
```bash
dotnet run
```

### エンドポイント
以下は主なエンドポイントの例です。

#### /ExitEntry
POSTリクエストを受け取り、売買注文を処理します。
- リクエストボディの例：
    ```json
    {
      "side": "1",
      "futureCode": "NK225",
      "derivMonth": 202109,
      "qty": 1
    }
    ```

#### /ExitOnly
POSTリクエストを受け取り、エグジット（決済）のみを処理します。
- リクエストボディの例：
    ```json
    {
      "side": "1",
      "futureCode": "NK225",
      "derivMonth": 202109,
      "qty": 1
    }
    ```

## 開発
### 主なクラスとメソッド
- `Program.cs`: アプリケーションのエントリーポイント。
  - `Exit`: 指定したポジションの決済処理を行います。
  - `Entry`: 新規の売買注文を行います。
  - `GetExChange`: 日中か夜間かを判定します。

## 貢献
バグ報告やプルリクエストは歓迎します。詳細は`CONTRIBUTING.md`を参照してください。

## ライセンス
このプロジェクトはMITライセンスの下で公開されています。詳細は`LICENSE`ファイルを参照してください。

## 著者
snowballrichdad
