using Microsoft.Playwright;
using NUnit.Framework;
using V1_WebTask.Utilities;

namespace V1_WebTask.Tests;

[TestFixture]
public class InterviewTests
{
    protected IPlaywright Playwright { get; private set; }
    protected IBrowser Browser { get; private set; }
    protected IBrowserContext BrowserContext { get; private set; }
    protected IPage Page { get; private set; }

    private readonly TestDataUtility _testData = new();

    [SetUp]
    public async Task SetUp()
    {
        // Initialize Playwright and browser before each test
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        BrowserContext = await Browser.NewContextAsync();
        Page = await BrowserContext.NewPageAsync();
        Page.SetDefaultTimeout(10000);
        await Page.GotoAsync(_testData.BaseUrl);
    }

    [TearDown]
    public async Task TearDown()
    {
        // Cleanup after each test
        await Task.WhenAll(Page.CloseAsync(), BrowserContext.CloseAsync(), Browser.CloseAsync());
        Playwright.Dispose();
    }

    // 1. Práce s Elementy
    // Otevřete webovou stránku, vyberte záložku "Kariéra" a najděte formulář "Kontaktujte nás".
    // Ověřte, že pokud není zaškrtnut souhlas se zpracováním osobních údajů, zobrazí se upozornění:
    // "Je třeba zaškrtnout políčko Souhlasím se zpracováním osobních údajů".
    [Test]
    public async Task ShouldShowError_WhenCheckboxUnchecked()
    {
        // Click on the "Kariéra" tab
        await Page.ClickAsync(_testData.GetSelector("CareerTab"));

        // Select the checkbox
        await Page.UncheckAsync(_testData.GetSelector("ConsentCheckbox"));

        // Attempt to submit the form without checking the consent checkbox
        await Page.ClickAsync(_testData.GetSelector("SubmitButton"));

        // Verify that the correct validation message is displayed
        var validationMessage = await Page.WaitForSelectorAsync(_testData.GetSelector("ValidationMessage"));
        Assert.AreEqual("Je třeba zaškrtnout políčko Souhlasím se zpracováním osobních údajů",
            await validationMessage.TextContentAsync(), "Validation message does not match expected value");
    }

    // 2. Vyplnění formuláře
    // Vyplňte formulář platnými údaji, včetně připojení souboru ve správném formátu, odešlete formulář a následně uzavřete potvrzovací okno.
    [Test]
    public async Task SubmitValidContactForm_ShouldSucceed()
    {
        await InsertValidDataIntoForm();

        // Wait for confirmation dialog to appear
        await Page.WaitForSelectorAsync(_testData.GetSelector("ConfirmationButton"));
        await Page.ClickAsync(_testData.GetSelector("ConfirmationButton"));
    }

    // 3. Ověření v databázi
    // Ověřte, že hodnoty zadané do formuláře byly správně uloženy v databázi
    [Test]
    public async Task SubmitValidContactForm_ShouldBePresentInDatabase()
    {
        // Insert valid data into the form
        await InsertValidDataIntoForm();

        var databaseUtility = new DatabaseUtility(_testData.DatabaseData);
        await databaseUtility.OpenConnectionAsync();

        var query = @$"
                SELECT COUNT(*)
                FROM {_testData.DatabaseData.TableName}
                WHERE 
                    name = @Name AND
                    email = @Email AND
                    phone = @Phone AND
                    message = @Message AND
                    file_name = @FileName";

        var count = (long)(await databaseUtility.ExecuteQueryAsync(query, _testData.FormData) ?? 0);
        Assert.IsTrue(count > 0, "Inserted form data was not found in the database");
    }

    private async Task InsertValidDataIntoForm()
    {
        // Click on the "Kariéra" tab
        await Page.ClickAsync(_testData.GetSelector("CareerTab"));

        // Select the checkbox
        await Page.CheckAsync(_testData.GetSelector("ConsentCheckbox"));

        // Fill out the form fields
        await Page.FillAsync(_testData.GetSelector("NameField"), _testData.FormData.Name);
        await Page.FillAsync(_testData.GetSelector("EmailField"), _testData.FormData.Email);
        await Page.FillAsync(_testData.GetSelector("PhoneField"), _testData.FormData.Phone);
        await Page.FillAsync(_testData.GetSelector("MessageField"), _testData.FormData.Message);
        await Page.SetInputFilesAsync(_testData.GetSelector("FileInput"), $"Utilities/{_testData.FormData.FileName}");

        // Submit the form
        await Page.ClickAsync(_testData.GetSelector("SubmitButton"));
    }
}
