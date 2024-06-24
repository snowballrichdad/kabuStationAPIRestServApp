using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using TaskScheduler;

namespace CSharp_sample
{
    // Response model
    public class TokenResponse
    {
        public string ResultCode { get; set; }
        public string Token { get; set; }
    }

    public class GenerateToken
    {
        // Get Token API
        public static string GetToken()
        {
            HttpClient client = new HttpClient();
            string Token = string.Empty;
            var obj = new
            {
                APIPassword = Constants.APIPassword
            };

            var url = Constants.Url + "/token";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.SendAsync(request).Result;

            // なぜかログインが切れている場合の対処
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // プロセス名を指定します
                string processName = "KabuS";

                // 指定した名前のすべてのプロセスを取得します
                Process[] processes = Process.GetProcessesByName(processName);

                foreach (Process process in processes)
                {
                    // プロセスを終了します
                    process.Kill();
                }

                ITaskService taskService = null;
                ITaskFolder rootFolder = null;
                IRegisteredTask registeredTask = null;

                // TaskServiceを生成して接続する
                taskService = new TaskScheduler.TaskScheduler();
                taskService.Connect();

                // タスクスケジューラーのルートフォルダを取得する
                rootFolder = taskService.GetFolder("\\");

                // 登録済みのタスクを取得する
                registeredTask = rootFolder.GetTask("kabuStationStartFromIIS");
                // タスクを実行する
                registeredTask?.Run(null);

                // 30秒間スリープします（10000ミリ秒 = 10秒）
                Thread.Sleep(30000);

                // リトライ
                request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                response = client.SendAsync(request).Result;
            }

            var JsonResult = JsonConvert.DeserializeObject<TokenResponse>(response.Content.ReadAsStringAsync().Result);

            Token = JsonResult.Token;

            return Token;
        }
    }
}
