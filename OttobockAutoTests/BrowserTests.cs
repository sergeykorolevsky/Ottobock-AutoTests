using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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
        string downloadFolder;

        private string password;
        private string login;

        string mainURLs = "https://testshop-us.corp.ottobock.int/";

        [SetUp]
        public void SetUp()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathDrivers = directory + "/../../../../drivers/";

            mainURL = mainURLs + "";
            authUrl = mainURLs + "login?referer=%2fstore%2fus01%2fen%2f";

            login = "test_h1_admin@ottobock.com";
            password = "Selenium1";

            downloadFolder = "C:\\Users\\KOROLEVSKY\\Downloads\\SeleniumTests"; //folder URL where files will be downloaded

            helperTest = new HelperTest();
            ChromeOptions options = new ChromeOptions();

            options.AddArguments("--no-sandbox");
            //options.AddArguments("--headless");
            options.AddArguments("--incognito");
            options.AddArguments("--ignore-ssl-errors=yes");
            options.AddArguments("--ignore-certificate-errors");

            options.AddUserProfilePreference("intl.accept_languages", "nl");
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
            options.AddUserProfilePreference("download.default_directory", @"C:\Users\KOROLEVSKY\Downloads\SeleniumTests");

            driver = new ChromeDriver(pathDrivers, options);

            driver.Manage().Cookies.DeleteAllCookies();
            driver.Manage().Window.Size = new System.Drawing.Size(1900, 956);
            //driver.Manage().Window.Maximize();
        }

        public void LoginToOttobock(IWebDriver driver, string urlSite, string authUrl, string login, string password, string mainURL)
        {
            driver.Url = urlSite;

            Thread.Sleep(1000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[2]/a");
            var SignIn = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[2]/a"));
            SignIn.Click();

            helperTest.waitElementId(driver, 60, "j_username");

            Assert.AreEqual(authUrl, driver.Url);
            Assert.AreEqual("Login | Ottobock US B2B Site", driver.Title);

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


        //[Test]
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

        //[Test]
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

            string ProductDetails = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[3]/div[1]/section/div")).Text;
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

        [Test]
        public void Shop()
        {
            //TS.US.Q.H1.1

            string bodyTextProduct;
            string priceNotDisplayed;

            driver.Url = mainURL;

            helperTest.waitElementId(driver, 60, "js-site-search-input");
            var SearchBox = driver.FindElement(By.Id("js-site-search-input"));
            SearchBox.Clear();
            SearchBox.SendKeys("3R60");

            Thread.Sleep(3000);

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("Modular Polycentric EBS Knee"));
            Assert.IsTrue(bodyTextProduct.Contains("3R60 Vacuum"));
            Assert.IsTrue(bodyTextProduct.Contains("EBS Knee Joint For Hip"));

            SearchBox.SendKeys(Keys.Enter);

            helperTest.waitElementId(driver, 60, "product-facet");
            Assert.AreEqual(mainURLs + "search/?text=3R60", driver.Url);

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("Modular Polycentric EBS Knee"));
            Assert.IsTrue(bodyTextProduct.Contains("3R60 Vacuum"));
            Assert.IsTrue(bodyTextProduct.Contains("EBS Knee Joint For Hip"));

            helperTest.JsClickElement(driver, "//*[text()='" + " Modular Polycentric EBS Knee Joint" + "']");

            helperTest.waitElementId(driver, 60, "js-bookmarkTabs");

            Assert.AreEqual(mainURLs + "Prosthetics/Lower-Limb-Prosthetics/Knees---Mechanical/Modular-Polycentric-EBS-Knee-Joint/p/3R60", driver.Url);

            priceNotDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/section/div/div[3]/div/div/div/div/p")).Text;
            Assert.IsEmpty(priceNotDisplayed);

            Thread.Sleep(3000);

            helperTest.waitElementId(driver, 60, "product-description-section");
            helperTest.waitElementId(driver, 60, "product-specification-section");
            helperTest.waitElementId(driver, 60, "product-references-accessories");
            helperTest.waitElementId(driver, 60, "product-references-sparepart");
            helperTest.waitElementId(driver, 60, "product-documents-section");

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[9]/div[8]/section/div/div/div[2]/div/div[1]/p[3]/a");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }
        }

        [Test]
        public void RequestQuotation()
        {
            //TS.US.Q.H1.2

            string priceDisplayed;
            string addToCartButton;
            string bodyTextProduct;
            string availableValue;
            string checkAvailable;
            decimal totalPrice;

            IWebElement InpBox;
            IWebElement PassBox;
            IWebElement CheckQuantity;

            driver.Url = mainURL + "/c/1300";

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[2]/a");

            helperTest.waitElementId(driver, 60, "j_username");

            Thread.Sleep(2000);

            InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            helperTest.waitElementId(driver, 60, "dLabel");
            driver.Url.Contains("/c/1300");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a");
            var SignIn = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a"));
            SignIn.Click();

            Thread.Sleep(3000);

            driver.Url = mainURL + "/p/3R60";

            helperTest.JsClickElementId(driver, "addToCartButton");
            helperTest.waitElementId(driver, 60, "j_username");

            Thread.Sleep(2000);

            InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            helperTest.waitElementId(driver, 60, "dLabel");
            driver.Url.Contains("/p/3R60");

            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/section/div/div[3]/div/div/div/div/p")).Text;
            Assert.IsNotEmpty(priceDisplayed);

            addToCartButton = driver.FindElement(By.Id("addToCartButton")).Text;
            Assert.AreEqual(addToCartButton, "ADD TO CART");

            Thread.Sleep(2000);

            //TS.US.Q.H1.3

            driver.Url = mainURL + "p/1C40~5K";

            Thread.Sleep(3000);

            helperTest.waitElementId(driver, 60, "configureProduct");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1C40=K"));

            helperTest.JsClickElementId(driver, "configureProduct");
            helperTest.waitElementId(driver, 60, "dynamicConfigContent");

            driver.Url.Contains("/p/1C40~5K/configuratorPage/CPQCONFIGURATOR");

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[3]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[8]/ul/li[3]/div")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[3]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[8]/ul/li[3]/div")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[4]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[9]/ul/li[2]/div")).Click();

            Thread.Sleep(3000);

            helperTest.JsClickElementId(driver, "exportCSV");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            helperTest.JsClickElementId(driver, "printCurrentConfiguration");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[2]/div/div/div/div/div/div[2]/div[2]/div[1]/button")).Click();

            helperTest.waitElementId(driver, 60, "configureProduct");

            driver.Url.Contains("/p/1C40~5K");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1C40=K"));

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");

            driver.Url.Contains("cart");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("1C40=L25-0-P/4"));
            Assert.IsTrue(bodyTextProduct.Contains("1C40=L25-0-P/0"));
            Assert.IsTrue(bodyTextProduct.Contains("2C4=L25/4"));

            //TS.US.Q.H1.4            

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div");
            var QuickOrderButton = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div"));
            QuickOrderButton.Click();

            Thread.Sleep(3000);

            driver.Url.Contains("quickOrder");

            InputAndCheckAdd("711S1=6X4", 2, "Bending Iron");
            InputAndCheckAdd("711s1=6x4", 3, "Bending Iron");
            InputAndCheckAdd("705S2=350", 4, "Shoemaker's Hammer");

            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[2]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);
            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[3]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);
            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[4]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);

            availableValue = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[2]/div[3]/div/span")).Text;
            checkAvailable = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[3]/div[3]/div/span")).Text;
            Assert.IsTrue(checkAvailable.Contains(availableValue));

            helperTest.JsClickElementId(driver, "js-add-to-cart-quick-order-btn-top");

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[3]/a/span");

            driver.Url.Contains("cart");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Bending Iron"));
            Assert.IsTrue(bodyTextProduct.Contains("Shoemaker's Hammer"));

            CheckQuantity = driver.FindElement(By.Id("quantity_3"));
            var Quantity1 = Convert.ToInt32(CheckQuantity.GetAttribute("value"));
            Assert.AreEqual(Quantity1, 2);

            //TS.US.Q.H1.5

            totalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[2]")).Text.Replace("$", ""));
            decimal checkTotalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[6]/div/span")).Text.Replace("$", ""));
            Assert.AreEqual(totalPrice, checkTotalPrice);

            var totalPriceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[10]/div[6]")).Text.Replace("$", ""));

            var sumTotalPriceItem = totalPriceItem1 + totalPriceItem2 + totalPriceItem3;

            Assert.AreEqual(totalPrice, sumTotalPriceItem);

            Thread.Sleep(1000);

            decimal price = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[2]/div/span")).Text.Replace("$", ""));

            var priceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[4]")).Text.Replace("$", ""));
            var priceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[4]")).Text.Replace("$", ""));
            var priceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[10]/div[4]")).Text.Replace("$", ""));

            var sumPriceItem = priceItem1 + (priceItem2 * 2) + priceItem3;

            Assert.AreEqual(price, sumPriceItem);

            Thread.Sleep(1000);

            decimal totalSaving = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[3]/div")).Text.Replace("- $", ""));

            var priceSaving = sumPriceItem - sumTotalPriceItem;

            Assert.AreEqual(totalSaving, priceSaving);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[5]/form/div[1]/span[1]")).Click();

            Thread.Sleep(3000);

            CheckQuantity = driver.FindElement(By.Id("quantity_3"));
            var ChangeQuantity = Convert.ToInt32(CheckQuantity.GetAttribute("value"));
            Assert.AreEqual(ChangeQuantity, 1);

            Thread.Sleep(1000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/button")).Click();
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/ul/li[1]/a")).Click();

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/button")).Click();
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/ul/li[2]/a")).Click();

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span");
            var RemoveItem = driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span"));
            RemoveItem.Click();

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[1]/div[1]");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsFalse(bodyTextProduct.Contains("1C40=L25-0-P/4"));
            Assert.IsFalse(bodyTextProduct.Contains("1C40=L25-0-P/0"));
            Assert.IsFalse(bodyTextProduct.Contains("2C4=L25/4"));

            Thread.Sleep(2000);

            //TS.US.Q.H1.6

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");
            driver.Url.Contains("cart");

            Thread.Sleep(2000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[5]/div/div[1]/div/div/button");
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[1]/div/div/button")).Click();

            helperTest.waitElementId(driver, 60, "selectcc");
            driver.Url.Contains("/checkout/multi/payment-type/choose");

            driver.FindElement(By.Id("selectcc")).Click();
            helperTest.waitElementId(driver, 60, "creditCardSelection");

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/div/section/div/a")).Click();

            helperTest.waitElementId(driver, 60, "tokenButton");
            driver.Url.Contains("prgwin.paymentsradius.com/PaymentsRadiusDI/v2/checkOut.do");

            helperTest.UseDropDownIdByName(driver, "inputCardType", "Visa");
            helperTest.InputStringId(driver, "4111111111111111", "cardNo1");
            helperTest.UseDropDownIdByName(driver, "cardExpMonth", "12");
            helperTest.UseDropDownIdByName(driver, "cardExpYear", "2040");
            helperTest.InputStringId(driver, "123", "cvvNumber");
            helperTest.InputStringId(driver, "Selenium Card Holder", "inputBillToFirstName");
            helperTest.InputStringId(driver, "Test-" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"), "inputBillToLastName");
            helperTest.InputStringId(driver, "11501 Alterra Pkwy Ste 600", "inputBillToStreet1");
            helperTest.InputStringId(driver, "Austin", "inputBillToCity");
            helperTest.UseDropDownIdByName(driver, "inputBillToCountry", "United States");
            helperTest.UseDropDownIdByName(driver, "inputBillToState", "Texas");
            helperTest.InputStringId(driver, "78758", "billToPostCode");
            helperTest.InputStringId(driver, "TEST", "inputBillToCompany");
            helperTest.InputStringId(driver, "", "inputBillToPhone");

            Thread.Sleep(1000);

            driver.FindElement(By.Id("tokenButton")).Click();

            Thread.Sleep(1000);
            helperTest.waitElementId(driver, 60, "responseFormSubmitButton");
            driver.FindElement(By.Id("responseFormSubmitButton")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/div/section/ul/li[last()]/aside/button")).Click();

            Thread.Sleep(1000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[1]/div[2]/a[1]/div[1]")).Click();

            Thread.Sleep(1000);

            helperTest.InputStringId(driver, "Selenium Test - " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"), "PurchaseOrderNumber");

            driver.FindElement(By.Id("choosePaymentType_continue_button")).Click();

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[1]/div[2]/div/div/div[2]/button")).Click();

            driver.FindElement(By.Id("newAddressDialogButton")).Click();

            helperTest.waitElementId(driver, 60, "address.firstName_del");

            helperTest.InputStringId(driver, "Test", "address.firstName_del");
            helperTest.InputStringId(driver, "Selenium", "address.surname_del");
            helperTest.InputStringId(driver, "1234 Test Lane", "address.line1_del");
            helperTest.InputStringId(driver, "Austin", "address.townCity_del");
            helperTest.InputStringId(driver, "78758", "address.postcode_del");
            helperTest.UseDropDownIdByName(driver, "regionSelectBox", "Texas");

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/form/div[2]/button")).Click();

            driver.FindElement(By.Id("deliveryMethodSubmit")).Click();

            helperTest.waitElementId(driver, 60, "placeInquiry");
            driver.Url.Contains("view");

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[1]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1111"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[2]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Selenium Test - " + DateTime.Now.ToString("yyyy-MM-dd")));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Test Selenium"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1234 Test Lane"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Austin, TX 78758"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("United States of America"));

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("711S1=6X4"));
            Assert.IsTrue(bodyTextProduct.Contains("705S2=350"));
            Assert.IsTrue(bodyTextProduct.Contains("SHG"));

            //TS.US.Q.H1.7

            driver.FindElement(By.Id("placeInquiry")).Click();

            Thread.Sleep(1000);

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/form/div[2]/input")).Click();

            Thread.Sleep(1000);

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("Thank you for creating a quote!"));
            driver.Url.Contains("Confirmation");
        }

        [Test]
        public void Order()
        {
            //TS.US.Q.H1.2

            string priceDisplayed;
            string addToCartButton;
            string bodyTextProduct;
            string availableValue;
            string checkAvailable;
            decimal totalPrice;

            IWebElement InpBox;
            IWebElement PassBox;
            IWebElement CheckQuantity;

            driver.Url = mainURL + "/c/1300";

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[2]/a");

            helperTest.waitElementId(driver, 60, "j_username");

            Thread.Sleep(2000);

            InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            helperTest.waitElementId(driver, 60, "dLabel");
            driver.Url.Contains("/c/1300");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a");
            var SignIn = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a"));
            SignIn.Click();

            Thread.Sleep(3000);

            driver.Url = mainURL + "/p/3R60";

            helperTest.JsClickElementId(driver, "addToCartButton");
            helperTest.waitElementId(driver, 60, "j_username");

            Thread.Sleep(2000);

            InpBox = driver.FindElement(By.Id("j_username"));
            InpBox.SendKeys(login);

            PassBox = driver.FindElement(By.Id("j_password"));
            PassBox.SendKeys(password);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[1]/div/div/div/form/button");

            helperTest.waitElementId(driver, 60, "dLabel");
            driver.Url.Contains("/p/3R60");

            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/section/div/div[3]/div/div/div/div/p")).Text;
            Assert.IsNotEmpty(priceDisplayed);

            addToCartButton = driver.FindElement(By.Id("addToCartButton")).Text;
            Assert.AreEqual(addToCartButton, "ADD TO CART");

            Thread.Sleep(2000);

            //TS.US.Q.H1.3

            driver.Url = mainURL + "p/1C40~5K";

            Thread.Sleep(3000);

            helperTest.waitElementId(driver, 60, "configureProduct");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1C40=K"));

            helperTest.JsClickElementId(driver, "configureProduct");
            helperTest.waitElementId(driver, 60, "dynamicConfigContent");

            driver.Url.Contains("/p/1C40~5K/configuratorPage/CPQCONFIGURATOR");

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[3]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[8]/ul/li[3]/div")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[3]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[8]/ul/li[3]/div")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[4]/div[2]/span/div")).Click();
            driver.FindElement(By.XPath("/html/body/div[9]/ul/li[2]/div")).Click();

            Thread.Sleep(3000);

            helperTest.JsClickElementId(driver, "exportCSV");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            helperTest.JsClickElementId(driver, "printCurrentConfiguration");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[2]/div/div/div/div/div/div[2]/div[2]/div[1]/button")).Click();

            helperTest.waitElementId(driver, 60, "configureProduct");

            driver.Url.Contains("/p/1C40~5K");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1C40=K"));

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");

            driver.Url.Contains("cart");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("1C40=L25-0-P/4"));
            Assert.IsTrue(bodyTextProduct.Contains("1C40=L25-0-P/0"));
            Assert.IsTrue(bodyTextProduct.Contains("2C4=L25/4"));

            //TS.US.Q.H1.4            

            helperTest.waitElementXpath(driver, 60, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div");
            var QuickOrderButton = driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[1]/a/div"));
            QuickOrderButton.Click();

            Thread.Sleep(3000);

            driver.Url.Contains("quickOrder");

            InputAndCheckAdd("711S1=6X4", 2, "Bending Iron");
            InputAndCheckAdd("711s1=6x4", 3, "Bending Iron");
            InputAndCheckAdd("705S2=350", 4, "Shoemaker's Hammer");

            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[2]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);
            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[3]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);
            priceDisplayed = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[4]/div[4]")).Text;
            Assert.IsNotEmpty(priceDisplayed);

            availableValue = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[2]/div[3]/div/span")).Text;
            checkAvailable = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div/div[3]/ul/li[3]/div[3]/div/span")).Text;
            Assert.IsTrue(checkAvailable.Contains(availableValue));

            helperTest.JsClickElementId(driver, "js-add-to-cart-quick-order-btn-top");

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[3]/a/span");

            driver.Url.Contains("cart");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Bending Iron"));
            Assert.IsTrue(bodyTextProduct.Contains("Shoemaker's Hammer"));

            CheckQuantity = driver.FindElement(By.Id("quantity_3"));
            var Quantity1 = Convert.ToInt32(CheckQuantity.GetAttribute("value"));
            Assert.AreEqual(Quantity1, 2);

            //TS.US.Q.H1.5

            totalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[2]")).Text.Replace("$", ""));
            decimal checkTotalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[6]/div/span")).Text.Replace("$", ""));
            Assert.AreEqual(totalPrice, checkTotalPrice);

            var totalPriceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[10]/div[6]")).Text.Replace("$", ""));

            var sumTotalPriceItem = totalPriceItem1 + totalPriceItem2 + totalPriceItem3;

            Assert.AreEqual(totalPrice, sumTotalPriceItem);

            Thread.Sleep(1000);

            decimal price = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[2]/div/span")).Text.Replace("$", ""));

            var priceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[4]")).Text.Replace("$", ""));
            var priceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[4]")).Text.Replace("$", ""));
            var priceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[10]/div[4]")).Text.Replace("$", ""));

            var sumPriceItem = priceItem1 + (priceItem2 * 2) + priceItem3;

            Assert.AreEqual(price, sumPriceItem);

            Thread.Sleep(1000);

            decimal totalSaving = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[3]/div")).Text.Replace("- $", ""));

            var priceSaving = sumPriceItem - sumTotalPriceItem;

            Assert.AreEqual(totalSaving, priceSaving);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[8]/div[5]/form/div[1]/span[1]")).Click();

            Thread.Sleep(3000);

            CheckQuantity = driver.FindElement(By.Id("quantity_3"));
            var ChangeQuantity = Convert.ToInt32(CheckQuantity.GetAttribute("value"));
            Assert.AreEqual(ChangeQuantity, 1);

            Thread.Sleep(1000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/button")).Click();
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/ul/li[1]/a")).Click();

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/button")).Click();
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[2]/div/div[2]/div/ul/li[2]/a")).Click();

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span");
            var RemoveItem = driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[2]/div[7]/div/form/button/span"));
            RemoveItem.Click();

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[1]/div[1]");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsFalse(bodyTextProduct.Contains("1C40=L25-0-P/4"));
            Assert.IsFalse(bodyTextProduct.Contains("1C40=L25-0-P/0"));
            Assert.IsFalse(bodyTextProduct.Contains("2C4=L25/4"));

            Thread.Sleep(2000);

            //TS.US.Q.H1.6

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");
            driver.Url.Contains("cart");

            Thread.Sleep(2000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div[3]/div[5]/div/div[1]/div/div/button");
            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[5]/div/div[1]/div/div/button")).Click();

            helperTest.waitElementId(driver, 60, "selectcc");
            driver.Url.Contains("/checkout/multi/payment-type/choose");

            driver.FindElement(By.Id("selectcc")).Click();
            helperTest.waitElementId(driver, 60, "creditCardSelection");

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/div/section/div/a")).Click();

            helperTest.waitElementId(driver, 60, "tokenButton");
            driver.Url.Contains("prgwin.paymentsradius.com/PaymentsRadiusDI/v2/checkOut.do");

            helperTest.UseDropDownIdByName(driver, "inputCardType", "Visa");
            helperTest.InputStringId(driver, "4111111111111111", "cardNo1");
            helperTest.UseDropDownIdByName(driver, "cardExpMonth", "12");
            helperTest.UseDropDownIdByName(driver, "cardExpYear", "2040");
            helperTest.InputStringId(driver, "123", "cvvNumber");
            helperTest.InputStringId(driver, "Selenium Card Holder", "inputBillToFirstName");
            helperTest.InputStringId(driver, "Test-" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"), "inputBillToLastName");
            helperTest.InputStringId(driver, "11501 Alterra Pkwy Ste 600", "inputBillToStreet1");
            helperTest.InputStringId(driver, "Austin", "inputBillToCity");
            helperTest.UseDropDownIdByName(driver, "inputBillToCountry", "United States");
            helperTest.UseDropDownIdByName(driver, "inputBillToState", "Texas");
            helperTest.InputStringId(driver, "78758", "billToPostCode");
            helperTest.InputStringId(driver, "TEST", "inputBillToCompany");
            helperTest.InputStringId(driver, "", "inputBillToPhone");

            Thread.Sleep(1000);

            driver.FindElement(By.Id("tokenButton")).Click();

            Thread.Sleep(1000);
            helperTest.waitElementId(driver, 60, "responseFormSubmitButton");
            driver.FindElement(By.Id("responseFormSubmitButton")).Click();

            Thread.Sleep(2000);

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/div/section/ul/li[last()]/aside/button")).Click();

            Thread.Sleep(1000);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[1]/div[2]/a[1]/div[1]")).Click();

            Thread.Sleep(1000);

            helperTest.InputStringId(driver, "Selenium Test - " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"), "PurchaseOrderNumber");

            driver.FindElement(By.Id("choosePaymentType_continue_button")).Click();

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[1]/div[2]/div/div/div[2]/button")).Click();

            driver.FindElement(By.Id("newAddressDialogButton")).Click();

            helperTest.waitElementId(driver, 60, "address.firstName_del");

            helperTest.InputStringId(driver, "Test", "address.firstName_del");
            helperTest.InputStringId(driver, "Selenium", "address.surname_del");
            helperTest.InputStringId(driver, "1234 Test Lane", "address.line1_del");
            helperTest.InputStringId(driver, "Austin", "address.townCity_del");
            helperTest.InputStringId(driver, "78758", "address.postcode_del");
            helperTest.UseDropDownIdByName(driver, "regionSelectBox", "Texas");

            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div[2]/div[2]/div[1]/div/form/div[2]/button")).Click();

            driver.FindElement(By.Id("deliveryMethodSubmit")).Click();

            helperTest.waitElementId(driver, 60, "placeInquiry");
            driver.Url.Contains("view");

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[1]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1111"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[2]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Selenium Test - " + DateTime.Now.ToString("yyyy-MM-dd")));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Test Selenium"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("1234 Test Lane"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Austin, TX 78758"));

            bodyTextProduct = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div[2]/div[2]/ul[1]/li[3]/div[2]")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("United States of America"));

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("711S1=6X4"));
            Assert.IsTrue(bodyTextProduct.Contains("705S2=350"));
            Assert.IsTrue(bodyTextProduct.Contains("SHG"));

            //TS.US.Q.H1.7

            driver.FindElement(By.Id("placeOrderForm1")).Click();

            helperTest.waitElementId(driver, 60, "accept-terms-button");
            driver.FindElement(By.Id("accept-terms-button")).Click();

            helperTest.waitElementId(driver, 60, "dLabel");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("Thank you for your Order!"));
            driver.Url.Contains("orderConfirmation");

            string orderNumber = driver.FindElement(By.XPath("/html/body/main/div[3]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div[2]/span")).Text;

            driver.FindElement(By.Id("dLabel")).Click();
            driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li[2]/div[1]/ul/li[3]/a")).Click();

            Thread.Sleep(3000);

            driver.Url.Contains("my-account/orders");

            helperTest.JsClickElement(driver, "//*[text()='" + (orderNumber) + "']");

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/a");
            driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/a")).Click();

            var browserTabs = driver.WindowHandles;
            driver.SwitchTo().Window(browserTabs[1]);

            Thread.Sleep(3000);

            driver.Url.Contains("/pdf/order/" + (orderNumber));

            driver.Close();
            driver.SwitchTo().Window(browserTabs[0]);
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        private bool CheckAndDeleteFile()
        {
            var directoryInfo = new DirectoryInfo(downloadFolder);
            var fileInfo = directoryInfo.GetFiles()?.OrderByDescending(p => p.CreationTime)?.FirstOrDefault();
            if (fileInfo != null)
            {
                File.Delete(fileInfo.FullName);
                return true;
            }
            else
            {
                return false;
            }
        }

        [TearDown]
        public void Cleanup()
        {
            driver?.Dispose();
        }
    }
}