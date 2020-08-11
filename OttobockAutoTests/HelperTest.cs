using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System.Linq;
using OpenQA.Selenium.IE;
using System.Text.RegularExpressions;
using System.Text;

namespace Selenium.Test
{
    class Values
    {
        public String item_category_link_uid;
        public String item_category_uid;
        public String sequence_no;
        public String link_name;
        public String full_link_path;
        public String isFound;


        public static Values FromCsv(string csvLine)
        {

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            String[] values = CSVParser.Split(csvLine);

            Values yValues = new Values();

            yValues.item_category_link_uid = Convert.ToString(values[0]);
            yValues.item_category_uid = Convert.ToString(values[1]);
            yValues.sequence_no = Convert.ToString(values[2]);
            yValues.link_name = Convert.ToString(values[3]);
            yValues.full_link_path = Convert.ToString(values[4]);

            return yValues;
        }
    }

    public class HelperTest
    {

        public void waitElementXpath(IWebDriver driver, int time, string Path)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            WebDriverWait wait = new WebDriverWait(driver, ts);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(Path)));
        }

        public void waitElementId(IWebDriver driver, int time, string Id)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            WebDriverWait wait = new WebDriverWait(driver, ts);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Id)));
            Thread.Sleep(400);
        }

        public void waitElementTagName(IWebDriver driver, int time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            WebDriverWait wait = new WebDriverWait(driver, ts);
            wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            Thread.Sleep(400);
        }

        public void JsClickElementId(IWebDriver driver, string Id)
        {

            waitElementId(driver, 60, Id);
            Thread.Sleep(500);
            IWebElement ele = driver.FindElement(By.Id(Id));
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", ele);
            Thread.Sleep(500);

        }

        public void FindTextInBody(IWebDriver driver, string Path)
        {
            String bodyText = driver.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(bodyText.Contains(Path));
        }

        public void NotFindTextInBody(IWebDriver driver, string Path)
        {
            waitElementTagName(driver, 60);
            String bodyText = driver.FindElement(By.TagName("body")).Text;
            Assert.IsFalse(bodyText.Contains(Path));
        }

        public void UseDropDown(IWebDriver driver, string path, int numOption)
        {
            waitElementXpath(driver, 60, path);

            IWebElement DropDown = driver.FindElement(By.XPath(path));
            Actions actions = new Actions(driver);
            actions.MoveToElement(DropDown).Build().Perform();
            string pathOpt = String.Concat(path, "/option[", numOption.ToString(), "]");
            var pushParam = driver.FindElement(By.XPath(pathOpt));
            pushParam.Click();
            Thread.Sleep(500);

        }
        public void UseDropDownByName(IWebDriver driver, string path, string nameOption)
        {
            waitElementXpath(driver, 60, path);

            IWebElement DropDown = driver.FindElement(By.XPath(path));
            var selectElement = new SelectElement(DropDown);
            selectElement.SelectByText(nameOption);

            Thread.Sleep(500);

        }
        public void UseDropDownIdByName(IWebDriver driver, string Id, string nameOption)
        {
            waitElementId(driver, 60, Id);

            IWebElement DropDown = driver.FindElement(By.Id(Id));
            var selectElement = new SelectElement(DropDown);
            selectElement.SelectByText(nameOption);

            Thread.Sleep(500);

        }
        public void UseDropDownId(IWebDriver driver, string path, int numOption)
        {
            waitElementId(driver, 60, path);

            IWebElement DropDown = driver.FindElement(By.Id(path));
            Actions actions = new Actions(driver);
            actions.MoveToElement(DropDown).Build().Perform();
            string pathOpt = String.Concat(path, "/option[", numOption.ToString(), "]");
            var pushParam = driver.FindElement(By.XPath(pathOpt));
            pushParam.Click();
            Thread.Sleep(500);

        }

        public void InputStringXpath(IWebDriver driver, string Value, string Path)
        {
            waitElementXpath(driver, 60, Path);
            IWebElement InpBox = driver.FindElement(By.XPath(Path));
            InpBox.Clear();
            InpBox.SendKeys(Value);
        }
        public void InputStringId(IWebDriver driver, string Value, string Path)
        {
            waitElementId(driver, 60, Path);
            IWebElement InpBox = driver.FindElement(By.Id(Path));
            InpBox.Clear();
            InpBox.SendKeys(Value);
        }

        public void JsClickElement(IWebDriver driver, string path)
        {

            waitElementXpath(driver, 120, path);
            Thread.Sleep(100);
            IWebElement ele = driver.FindElement(By.XPath(path));
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", ele);
            Thread.Sleep(500);
        }

        public void JsClickElementID(IWebDriver driver, string path)
        {

            //waitElementXpath(driver, 120, path);
            Thread.Sleep(100);
            IWebElement ele = driver.FindElement(By.Id(path));
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", ele);
            Thread.Sleep(500);
        }

        public void LoginToSite(IWebDriver driver, string urlSite, string homeUrl, string login, string password, string mainURL)
        {
            if ((driver.Url.Contains(mainURL)) & (!(driver.Url.Contains("auth"))))
            {

            }
            else
            {
                driver.Url = urlSite;

                if (driver.Url != homeUrl)
                {
                    waitElementId(driver, 60, "input-mail");

                    Thread.Sleep(3000);

                    IWebElement InpBox = driver.FindElement(By.Id("input-mail"));
                    InpBox.SendKeys(login);

                    IWebElement PassBox = driver.FindElement(By.Id("input-pass"));
                    PassBox.SendKeys(password);

                    JsClickElement(driver, "//*[text()='" + " Login " + "']");

                    Thread.Sleep(2000);
                }

            }        
        }
    }
}

