using AsyncChromeDriverConsoleApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


using BaristaLabs.ChromeDevTools.Runtime;
using Page = BaristaLabs.ChromeDevTools.Runtime.Page;
using Network = BaristaLabs.ChromeDevTools.Runtime.Network;
using System.Collections.Concurrent;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;

namespace chrome_dev_tools_sample_async_test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChromeSession session;
        private ConcurrentDictionary<string, RequestInfo> requestsDic = new ConcurrentDictionary<string, RequestInfo>();
        private ObservableCollection<RequestInfo> requestsCollection = new ObservableCollection<RequestInfo>();
        SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public MainWindow()
        {
            InitializeComponent();
            lbRequests.ItemsSource = requestsCollection;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //Launch Chrome With

            //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223
            //"C:\Program Files\Opera\47.0.2631.55\opera.exe" --remote-debugging-port=9223

            //Console.WriteLine("Hello World!");

            var sessions = await GetSessions("http://localhost:9223/"); //.GetAwaiter().GetResult();

            session = new ChromeSession(sessions.First(s => s.Type == "page").WebSocketDebuggerUrl);
            try
            {
                session.Network.SubscribeToResponseReceivedEvent((e2) =>
                {
                    var url = e2.Response.Url;
                    RequestInfo reqInfo = new RequestInfo(DateTime.Now, url, e2.RequestId);
                    reqInfo.DataType = e2.Type.ToString();
                    requestsDic.GetOrAdd(e2.RequestId, reqInfo);

                });

                session.Network.SubscribeToLoadingFinishedEvent(OnLoadingFinishedEvent);

                await session.Network.Enable(new Network.EnableCommand());

                var navigateResult = await session.Page.Navigate(new Page.NavigateCommand
                {
                    Url = "https://www.google.com/"
                });
                //Console.ReadLine();

            }
            catch (Exception ex)
            {
                tbRequestData.Text = ex.ToString();
            }

        }

        private void OnLoadingFinishedEvent(Network.LoadingFinishedEvent e2)
        {
            Task.Run(async () =>
            {
                var reqId = e2.RequestId;
                if (requestsDic.TryGetValue(reqId, out RequestInfo reqInfo))
                {
                    try
                    {
                        // WORKS only when one thread ask GetResponseBody
                        // If many threads, gives wrong results. Try comment // semaphore.Wait(); and look requests body 
                        // Not correct bodies in some requests
                        semaphore.Wait();
                        var body = await session.Network.GetResponseBody(new Network.GetResponseBodyCommand { RequestId = reqId });
                        if (body.Base64Encoded)
                        {
                            reqInfo.Body = Encoding.Default.GetString(Convert.FromBase64String(body.Body));
                        }
                        else
                        {
                            reqInfo.Body = body.Body;
                        }
                    }
                    catch (Exception ex)
                    {
                        reqInfo.Body = ex.ToString();
                    }
                    finally
                    {
                        semaphore.Release();
                    }


                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        requestsCollection.Add(reqInfo);
                    });
                }
            });
        }


        public static async Task<ICollection<ChromeSessionInfo>> GetSessions(string chromeUrl)
        {
            using (var webClient = new HttpClient())
            {
                webClient.BaseAddress = new Uri(chromeUrl);
                var remoteSessions = await webClient.GetStringAsync("/json");
                return JsonConvert.DeserializeObject<ICollection<ChromeSessionInfo>>(remoteSessions);
            }
        }

        private void lbRequests_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var req = lbRequests.SelectedItem as RequestInfo;
            tbRequestData.Text = req.Id + "  " + req.DataType + Environment.NewLine +
                                req.Url + Environment.NewLine + Environment.NewLine +
                                req.Body;
        }
    }
}
