using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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

            downloadFolder = "C:\\Users\\working-pc-1\\Downloads\\SeleniumTests"; //folder URL where files will be downloaded

            helperTest = new HelperTest();
            ChromeOptions options = new ChromeOptions();

            options.AddArguments("--no-sandbox");
            //options.AddArguments("--headless");
            options.AddArguments("--incognito");

            options.AddUserProfilePreference("intl.accept_languages", "nl");
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
            options.AddUserProfilePreference("download.default_directory", @"C:\Users\working-pc-1\Downloads\SeleniumTests");

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
            Assert.IsTrue(bodyTextProduct.Contains("EBS-PRO-Knee"));

            SearchBox.SendKeys(Keys.Enter);

            helperTest.waitElementId(driver, 60, "product-facet");
            Assert.AreEqual(mainURLs + "search/?text=3R60", driver.Url);

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("Modular Polycentric EBS Knee"));
            Assert.IsTrue(bodyTextProduct.Contains("3R60 Vacuum"));
            Assert.IsTrue(bodyTextProduct.Contains("EBS Knee Joint For Hip"));
            Assert.IsTrue(bodyTextProduct.Contains("EBS-PRO-Knee"));

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

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[3]/div[7]/section/div/div/div[2]/div/div[1]/p[3]/a");

            Thread.Sleep(3000);

            if (!CheckAndDeleteFile())
            {
                Assert.Fail();
            }
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

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[2]/div/ul/li/a");

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

            driver.Url = mainURL + "/p/3C88-3~59~82";

            Thread.Sleep(3000);

            helperTest.waitElementId(driver, 60, "configureProduct");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("3C88-3=9.2"));

            helperTest.JsClickElementId(driver, "configureProduct");
            helperTest.waitElementId(driver, 60, "dynamicConfigContent");

            driver.Url.Contains("/p/3C88-3~59~82/configuratorPage/CPQCONFIGURATOR");

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[3]/div[2]/div[1]/div/input");
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[5]/div[2]/div[2]/div/input");
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[6]/div[2]/div[2]/div/input");

            Thread.Sleep(1000);

            helperTest.waitElementXpath(driver, 60, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[7]/div[2]/input");
            IWebElement InputBox = driver.FindElement(By.XPath("/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[7]/div[2]/input"));
            InputBox.Clear();
            InputBox.SendKeys("100");

            Thread.Sleep(1000);

            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[8]/div[2]/div[3]/div/input");
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[9]/div[2]/div[1]/div/input");
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[10]/div[2]/div[2]/div/input");
            helperTest.JsClickElement(driver, "/html/body/main/div[3]/div/div[2]/div[3]/div[1]/div[2]/div/form/div[1]/div[2]/div[14]/div[2]/div[3]/div/input");

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

            driver.Url.Contains("/p/3C88-3~59~82");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyTextProduct.Contains("3C88-3=9.2"));

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");

            driver.Url.Contains("cart");

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(bodyTextProduct.Contains("3C2000"));
            Assert.IsTrue(bodyTextProduct.Contains("3C98-3"));
            Assert.IsTrue(bodyTextProduct.Contains("2R57"));
            Assert.IsTrue(bodyTextProduct.Contains("SP-3C98-3=3"));
            Assert.IsTrue(bodyTextProduct.Contains("646D1022=EN_US"));

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

            CheckQuantity = driver.FindElement(By.Id("quantity_5"));
            var Quantity1 = Convert.ToInt32(CheckQuantity.GetAttribute("value"));
            Assert.AreEqual(Quantity1, 2);

            //TS.US.Q.H1.5

            totalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[2]")).Text.Replace("$", ""));
            decimal checkTotalPrice = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[6]/div/span")).Text.Replace("$", ""));
            Assert.AreEqual(totalPrice, checkTotalPrice);

            var totalPriceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[5]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[7]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[9]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem4 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[11]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem5 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[13]/div[6]")).Text.Replace("$", ""));
            var totalPriceItem6 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[15]/div[6]")).Text.Replace("$", ""));

            var sumTotalPriceItem = totalPriceItem1 + totalPriceItem2 + totalPriceItem3 + totalPriceItem4 + totalPriceItem5 + totalPriceItem6;

            Assert.AreEqual(totalPrice, sumTotalPriceItem);

            Thread.Sleep(1000);

            decimal price = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[2]/div/span")).Text.Replace("$", ""));

            var priceItem1 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[5]/div[4]")).Text.Replace("$", ""));
            var priceItem2 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[7]/div[4]")).Text.Replace("$", ""));
            var priceItem3 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[9]/div[4]")).Text.Replace("$", ""));
            var priceItem4 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[11]/div[4]")).Text.Replace("$", ""));
            var priceItem5 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[13]/div[4]")).Text.Replace("$", ""));
            var priceItem6 = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[15]/div[4]")).Text.Replace("$", ""));

            var sumPriceItem = priceItem1 + priceItem2 + priceItem3 + priceItem4 + (priceItem5 * 2) + priceItem6;

            Assert.AreEqual(price, sumPriceItem);

            Thread.Sleep(1000);

            decimal totalSaving = Convert.ToDecimal(driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[4]/div[2]/div/div/div/div[3]/div")).Text.Replace("- $", ""));

            var priceSaving = sumPriceItem - sumTotalPriceItem;

            Assert.AreEqual(totalSaving, priceSaving);

            driver.FindElement(By.XPath("/html/body/main/div[3]/div[3]/div[1]/div/ul/li[13]/div[5]/form/div[1]/span[1]")).Click();

            Thread.Sleep(3000);

            CheckQuantity = driver.FindElement(By.Id("quantity_5"));
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

            Thread.Sleep(5000);

            bodyTextProduct = driver.FindElement(By.TagName("body")).Text;

            Assert.IsFalse(bodyTextProduct.Contains("3C2000"));
            Assert.IsFalse(bodyTextProduct.Contains("3C98-3"));
            Assert.IsFalse(bodyTextProduct.Contains("2R57"));
            Assert.IsFalse(bodyTextProduct.Contains("SP-3C98-3=3"));
            Assert.IsFalse(bodyTextProduct.Contains("646D1022=EN_US"));
        }

        [Test]
        public void CheckOut()
        {
            //TS.US.Q.H1.6

            LoginToOttobock(driver, mainURL, authUrl, login, password, mainURL);

            Thread.Sleep(2000);

            helperTest.JsClickElement(driver, "/html/body/main/header/nav[1]/div/div[2]/div[3]/ul/li[3]/div/div[2]/div[1]/a/div[1]/span");

            helperTest.waitElementId(driver, 60, "cartEntryActionForm");
            driver.Url.Contains("cart");

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
            helperTest.InputStringId(driver, "Test-" + DateTime.Now.ToString("yyyy-MM-dd"), "inputBillToLastName");
            helperTest.InputStringId(driver, "11501 Alterra Pkwy Ste 600", "inputBillToStreet1");
            helperTest.InputStringId(driver, "Austin", "inputBillToCity");
            helperTest.UseDropDownIdByName(driver, "inputBillToCountry", "United States");
            helperTest.UseDropDownIdByName(driver, "inputBillToState", "Texas");
            helperTest.InputStringId(driver, "78758", "billToPostCode");
            helperTest.InputStringId(driver, "TEST", "inputBillToCompany");

            Thread.Sleep(1000);

            driver.FindElement(By.Id("tokenButton")).Click();

            Thread.Sleep(1000);
            helperTest.waitElementId(driver, 60, "responseFormSubmitButton");
            driver.FindElement(By.Id("responseFormSubmitButton")).Click();

            Thread.Sleep(1000);
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




