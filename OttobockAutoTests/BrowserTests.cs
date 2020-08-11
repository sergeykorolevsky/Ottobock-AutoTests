using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Selenium.Test
{

    [TestFixture]
    public class OttobockTests
    {
        private static IWebDriver driver;
        private static HelperTest helperTest;
        string mainURL;
        string authUrl;

        private string password;
        private string login;

        string mainURLs = "https://shop.ottobock.us/";

        [SetUp]
        public void SetUp()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathDrivers = directory + "/../../../../drivers/";

            mainURL = mainURLs + "";
            authUrl = mainURLs + "login?referer=%2fstore%2fus01%2fen%2f";

            login = "test_h1_admin@ottobock.com";
            password = "Selenium1";

            helperTest = new HelperTest();
            ChromeOptions options = new ChromeOptions();

            options.AddArguments("--no-sandbox");
            //options.AddArguments("--headless");                        

            options.AddUserProfilePreference("intl.accept_languages", "nl");
            options.AddUserProfilePreference("disable-popup-blocking", "true");

            driver = new ChromeDriver(pathDrivers, options);

            driver.Manage().Cookies.DeleteAllCookies();
            driver.Manage().Window.Size = new System.Drawing.Size(1920, 1024);
            //driver.Manage().Window.Maximize();
        }

        public void LoginToOttobock(IWebDriver driver, string urlSite, string authUrl, string login, string password, string mainURL)
        {
            driver.Url = urlSite;

            Thread.Sleep(1000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a");
            var SignIn = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a"));
            SignIn.Click();

            Assert.AreEqual(authUrl, driver.Url);
            Assert.AreEqual("Login | Ottobock US B2B Site", driver.Title);

            helperTest.waitElementId(driver, 60, "j_username");

            Thread.Sleep(2000);

            IWebElement InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            IWebElement PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            Thread.Sleep(2000);
        }

        [Test]
        public void Login()
        {
            LoginToOttobock(driver, mainURL, authUrl, login, password, mainURL);

            Thread.Sleep(1000);

            helperTest.waitElementId(driver, 60, "dLabel");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[3]/a");
            var SignOut = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[3]/a"));
            SignOut.Click();

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a");
            Assert.AreEqual(mainURL, driver.Url);

            Thread.Sleep(1000);
        }

        public void InputAndCheckAdd(string productId, int num, string nameCheck)
        {
            string numInput = "/html/body/main/div[3]/div/div/div[3]/ul/li[" + num.ToString() + "]/div[1]/input[1]";
            IWebElement InpBox = driver.FindElement(By.XPath(numInput));
            InpBox.Clear();
            InpBox.SendKeys(productId);

            Thread.Sleep(2000);

            driver.FindElement(By.XPath(numInput)).SendKeys(Keys.Enter);

            Thread.Sleep(4000);

            String bodyText = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyText.Contains(nameCheck));
        }


        [Test]
        public void QuickOrder()
        {
            LoginToOttobock(driver, mainURL, authUrl, login, password, mainURL);

            Thread.Sleep(1000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div");
            var QuickOrderButton = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div"));
            QuickOrderButton.Click();

            Thread.Sleep(3000);

            Assert.AreEqual(mainURLs + "quickOrder", driver.Url);

            InputAndCheckAdd("6Y81=320-10", 2, "ProSeal SIL Liner");
            InputAndCheckAdd("6Y110=300X10", 3, "Skeo Sealing Liner");
            InputAndCheckAdd("50K30=L-XS-7-1", 4, "Xeleton");

            helperTest.JsClickElementId(driver, "js-add-to-cart-quick-order-btn-top");

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[3]/a/span");

            Assert.AreEqual(mainURLs + "cart", driver.Url);

            String ItemText = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(ItemText.Contains("ProSeal SIL Liner"));
            Assert.IsTrue(ItemText.Contains("Skeo Sealing"));
            Assert.IsTrue(ItemText.Contains("Xeleton"));

            for (int i = 0; i < 3; i++)
            {
                helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span");
                var RemoveItem = driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span"));
                RemoveItem.Click();
            }

            Thread.Sleep(2000);
        }

        [Test]
        public void ProductSearch()
        {
            driver.Url = mainURL;

            helperTest.waitElementId(driver, 60, "js-site-search-input");
            var SearchBox = driver.FindElement(By.Id("js-site-search-input"));
            SearchBox.Clear();
            SearchBox.SendKeys("1c60");
            SearchBox.SendKeys(Keys.Enter);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[1]/div[2]/div/div/div/div[4]/div/div[1]/div/div[1]/a/img");
            Assert.AreEqual(mainURLs + "search/?text=1c60", driver.Url);
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div[1]/div[2]/div/div/div/div[4]/div/div[1]/div/div[1]/a/img");

            Thread.Sleep(3000);

            Assert.AreEqual(mainURLs + "Prosthetics/Lower-Limb-Prosthetics/Feet---Mechanical/1C60-Triton/p/1C60", driver.Url);

            string Price = driver.FindElement(By.TagName("body")).Text;
            Assert.IsFalse(Price.Contains("$1,560.00"));

            string ProductDetails = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[3]/div/div[2]/div/div/div/div/div/div/p[1]")).Text;
            Assert.IsTrue(ProductDetails.Contains("The 1C60 Triton"));

            helperTest.JsClickElement(driver, "//*[text()='" + "Spare Parts" + "']");

            Thread.Sleep(2000);

            string SpareParts = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(SpareParts.Contains("2C20"));

            helperTest.JsClickElement(driver, "//*[text()='" + "Documents" + "']");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div/div[3]/div/div[8]/div/div/div/div[1]/div/div[2]/div[1]/h3");
            string Document = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(Document.Contains("Function Matrix – Prosthetic Feet"));

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[3]/div/div[8]/div/div/div/div[1]/div/div[2]/div[2]/ul/li/a/span");

            var browserTabs = driver.WindowHandles;
            driver.SwitchTo().Window(browserTabs[1]);

            Thread.Sleep(6000);

            Assert.AreEqual(mainURLs + "media/pdf/646F307-EN-05-1209w.pdf", driver.Url);

            driver.Close();
            driver.SwitchTo().Window(browserTabs[0]);

            helperTest.JsClickElementId(driver, "addToCartButton");

            helperTest.waitElementId(driver, 60, "j_username");

            Assert.AreEqual(mainURLs + "login?referer=%2fstore%2fus01%2fen%2fProsthetics%2fLower-Limb-Prosthetics%2fFeet---Mechanical%2f1C60-Triton%2fp%2f1C60", driver.Url);

            IWebElement InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            IWebElement PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            helperTest.waitElementId(driver, 60, "configureProduct");

            Assert.AreEqual(mainURLs + "Prosthetics/Lower-Limb-Prosthetics/Feet---Mechanical/1C60-Triton/p/1C60", driver.Url);

            string ShowPrice = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(ShowPrice.Contains("$1,560.00"));

            Thread.Sleep(2000);

            driver.FindElement(By.Id("configureProduct"));

            Thread.Sleep(2000);
        }

        [TearDown]
        public void Cleanup()
        {
            driver?.Dispose();
        }
    }
}




