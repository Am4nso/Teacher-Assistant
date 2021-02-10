using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teacher_Assistant
{

    class Schoology
    {

        public static ChromeOptions options = new ChromeOptions();

        public static IEnumerable<System.Net.Cookie> responseCookies;
        public static IWebDriver chrome;

        public static string program_path = Path.GetTempPath() + "teacher_assistant/";

        private static readonly CookieContainer cookies = new CookieContainer();

        public static Dictionary<string, Dictionary<string, string>> schedule = new Dictionary<string, Dictionary<string, string>>() { {"Sunday", new Dictionary<string, string>()},
                                                                                                                                       {"Monday", new Dictionary<string, string>()},
                                                                                                                                       {"Tuesday", new Dictionary<string, string>()},
                                                                                                                                       {"Wednesday", new Dictionary<string, string>()},
                                                                                                                                       {"Thursday", new Dictionary<string, string>()}};
        public static Dictionary<string, string> course_to_id = new Dictionary<string, string>();


        public Schoology()
        {

            if (!Directory.Exists(program_path))
            {
                Directory.CreateDirectory(program_path);
            }

            if (!File.Exists(program_path + "chromedriver.exe"))
            {
                byte[] bytes = Properties.Resources.chromedriver;
                File.WriteAllBytes(program_path + "chromedriver.exe", bytes);
            }

            if (File.Exists(program_path + "schedule.json"))
            {
                string scheduleText = File.ReadAllText(program_path + "schedule.json");

                schedule = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(scheduleText);
            }

            if (File.Exists(Schoology.program_path + "courses.json"))
            {
                string coursesText = File.ReadAllText(program_path + "courses.json");

                course_to_id = JsonConvert.DeserializeObject<Dictionary<string, string>>(coursesText);
            }

            options.AddArgument("start-maximized");
            options.AddArgument("headless");
            options.AddArgument("mute-audio");

            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(program_path);
            driverService.HideCommandPromptWindow = true;

            chrome = new ChromeDriver(driverService, options);

        }

        public static async Task<bool> LogIn(string username, string password, ProgressBar bar=null)
        {
            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 1;
                });
            }

            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = cookies
            };

            HttpClient client = new HttpClient(handler);

            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 10;
                });
            }

            string responseString = await client.GetStringAsync("https://inpsa.schoology.com/login?&school=2382278049");

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

            document.LoadHtml(responseString);

            string form_id = document.DocumentNode.Descendants("input").Where(node => node.Attributes["name"].Value == "form_build_id").FirstOrDefault().Attributes["value"].Value;

            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 50;
                });
            }

            var values = new Dictionary<string, string>
            {
                { "mail", username },
                { "pass", password },
                { "school_nid", "2382278049" },
                { "form_id", "s_user_login_form" },
                { "form_build_id", form_id },
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://inpsa.schoology.com/login?&school=2382278049", content);

            var responseStringPOST = await response.Content.ReadAsStringAsync();

            document.LoadHtml(responseStringPOST);

            if (document.GetElementbyId("login-container") != null)
            {
                return false;
            }

            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 50;
                });
            }

            responseCookies = cookies.GetCookies(new Uri("https://inpsa.schoology.com")).Cast<System.Net.Cookie>();

            chrome.Url = "https://inpsa.schoology.com/login?&school=2382278049";

            foreach (System.Net.Cookie cookie in responseCookies)
            {
                chrome.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value));
            }

            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 75;
                });
            }

            chrome.Url = "https://inpsa.schoology.com/login?&school=2382278049";

            client.Dispose();

            if (bar != null)
            {
                bar.Invoke((MethodInvoker)delegate {
                    bar.Value = 100;
                });
            }

            return true;
        }

        public static string NextPeriodIn()
        {
            DateTime now = DateTime.Now;

            DateTime finalTime = new DateTime(now.Year, now.Month, now.Day, 13, 10, 0);

            int daystoadd = 0;

            string day;

            if (now > finalTime)
            {
                daystoadd++;
                day = DateTime.Now.AddDays(1).ToString("dddd");
            }
            else
            {
                day = DateTime.Now.ToString("dddd");
            }

            switch (day)
            {
                case "Saturday":
                    daystoadd++;
                    day = "Sunday";
                    break;
                case "Friday":
                    daystoadd += 2;
                    day = "Sunday";
                    break;
            }

            Dictionary<string, string> todaySchedule = schedule[day];

            DateTime nextPeriod = now;

            foreach (KeyValuePair<string, string> entry in todaySchedule)
            {
                string time = entry.Key;

                string hours = time.Substring(0, time.IndexOf(":"));

                string minutes = time.Substring(time.IndexOf(":") + 1);

                DateTime fakeTime = new DateTime(now.Year, now.Month, now.Day + daystoadd, int.Parse(hours), int.Parse(minutes), 0);

                if (fakeTime >= now)
                {
                    nextPeriod = fakeTime;
                    break;
                }

            }

            TimeSpan margin = TimeSpan.FromSeconds((nextPeriod - now).TotalSeconds);
            string answer = string.Format("{0:D1}:{1:D2}:{2:D2}:{3:D2}",
                                            margin.Days,
                                            margin.Hours,
                                            margin.Minutes,
                                            margin.Seconds);

            return answer;

        }

        public static string NextPeriod()
        {
            DateTime now = DateTime.Now;

            DateTime finalTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0);

            int daystoadd = 0;

            string day;

            if (now > finalTime)
            {
                day = DateTime.Now.AddDays(1).ToString("dddd");
                daystoadd++;
            }
            else
            {
                day = DateTime.Now.ToString("dddd");
            }

            switch (day)
            {
                case "Saturday":
                    daystoadd++;
                    day = "Sunday";
                    break;
                case "Friday":
                    daystoadd += 2;
                    day = "Sunday";
                    break;
            }

            Dictionary<string, string> todaySchedule = schedule[day];

            string nextPeriod = null;

            foreach (KeyValuePair<string, string> entry in todaySchedule)
            {
                string time = entry.Key;

                string hours = time.Substring(0, time.IndexOf(":"));

                string minutes = time.Substring(time.IndexOf(":") + 1);

                DateTime fakeTime = new DateTime(now.Year, now.Month, now.Day + daystoadd, int.Parse(hours), int.Parse(minutes), 0);

                if (fakeTime >= now)
                {
                    nextPeriod = entry.Value;
                    break;
                }

            }

            return nextPeriod;
        }

        public static string CurrentPeriod()
        {
            DateTime now = DateTime.Now;

            DateTime finalTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0);

            string day = DateTime.Now.ToString("dddd");

            switch (day)
            {
                case "Saturday":
                    return null;
                case "Friday":
                    return null;
            }

            if (now >= finalTime)
            {
                return null;
            }

            Dictionary<string, string> todaySchedule = schedule[day];

            string currentPeriod = null;

            foreach (KeyValuePair<string, string> entry in todaySchedule)
            {
                string time = entry.Key;

                string hours = time.Substring(0, time.IndexOf(":"));

                string minutes = time.Substring(time.IndexOf(":") + 1);

                DateTime fakeTime = new DateTime(now.Year, now.Month, now.Day, int.Parse(hours), int.Parse(minutes), 0);

                if (now >= fakeTime)
                {
                    currentPeriod = entry.Value;
                }
                else
                {
                    break;
                }

            }

            return currentPeriod;
        }

        public static ArrayList GetUserList()
        {
            ArrayList list = new ArrayList();

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

            document.LoadHtml(chrome.PageSource);

            var users = document.DocumentNode.SelectNodes("//div[contains(@class, 'userName--6aS3s')]");

            if (users == null)
            {
                return list;
            }

            foreach (HtmlAgilityPack.HtmlNode i in users)
            {
                list.Add(i.Attributes["aria-label"].Value.Replace("    Status none", "").Replace(" Presenter   Status none", ""));
            }

            return list;
        }

        public static async Task<ArrayList> GetStudentsAsync(string class_name)
        {
            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync("https://raw.githubusercontent.com/Am4nso/schoology-simplified-database/main/students/" + class_name.Replace(" ", "-") + ".json");

            ArrayList list = JsonConvert.DeserializeObject<ArrayList>(response);

            client.Dispose();

            return list;
        }

        public static bool JoinConference(string course_id)
        {
            chrome.Url = "https://inpsa.schoology.com/apps/login/saml/initial?realm=course&realm_id=" + course_id + "&spentityid=9295f7b9ba9a31af8c09d5442f697eb005452c17d&RelayState=https%3A%2F%2Fbigbluebutton.app.schoology.com%2Fhome%3Frealm%3Dsection%26realm_id%3D" + course_id + "%26app_id%3D191034318%26is_ssl%3D1";

            Thread.Sleep(6000);

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

            document.LoadHtml(chrome.PageSource);

            HtmlAgilityPack.HtmlNode conference = document.DocumentNode.Descendants("tr").Where(node => node.HasClass("conference-row")).FirstOrDefault();

            if (conference == null)
            {
                return false;
            }

            string is_running = conference.Descendants("span").Where(node => node.HasClass("conference-status")).FirstOrDefault().InnerText;

            if (is_running == "Not started")
            {
                return false;
            }

            var element = conference.Descendants("a").Where(node => node.HasClass("ng-binding")).FirstOrDefault();

            while (element == null)
            {
                element = conference.Descendants("a").Where(node => node.HasClass("ng-binding")).FirstOrDefault();
            }

            string code = element.Attributes["href"].Value;

            chrome.Url = "https://bigbluebutton.app.schoology.com/" + code;

            Thread.Sleep(10000);

            chrome.FindElement(By.XPath("//button[@aria-label='Listen only']")).Click();

            return true;
        }

        public static bool HasEnded()
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

            document.LoadHtml(chrome.PageSource);

            var users = document.DocumentNode.SelectNodes("//div[contains(@class, 'starRating--mAjpr')]");

            return users != null;
        }


    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize the program
            Schoology schoology = new Schoology();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            if (File.Exists(Schoology.program_path + "information.json"))
            {
                Application.Run(new Loading());
            }

            Application.Run(new Login());
        }
    }
}
