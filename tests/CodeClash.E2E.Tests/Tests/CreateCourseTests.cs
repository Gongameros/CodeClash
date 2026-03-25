using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class CreateCourseTests : BaseE2ETest
{
    public CreateCourseTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateCourse_FullWizardFlow_CreatesSuccessfully()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        var data = TestCourseData.Default();

        // Step 0: Basic Info
        await createPage.FillBasicInfoAsync(data.Title, data.Description);
        await createPage.GoToNextStepAsync();

        // Step 1: Technologies
        foreach (var tech in data.Technologies)
            await createPage.SelectTechAsync(tech);
        await createPage.GoToNextStepAsync();

        // Step 2: Tags
        foreach (var tag in data.Tags)
            await createPage.AddTagAsync(tag);
        await createPage.GoToNextStepAsync();

        // Step 3: Review & Submit
        var reviewText = await Page.ContentAsync();
        reviewText.ShouldContain(data.Title);

        await createPage.SubmitAsync();

        // Should navigate to the course detail page
        await Page.WaitForURLAsync(
            url => url.Contains("/courses/") && !url.TrimEnd('/').EndsWith("/courses/new"),
            new PageWaitForURLOptions { Timeout = 15_000 });
        Page.Url.ShouldContain("/courses/");

        await WaitForSnackbarAsync("Course created successfully!");
    }

    [Fact]
    public async Task Step0_NextButtonDisabled_WithoutTitleAndDescription()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        // Next button should be disabled initially
        (await createPage.NextButton.IsDisabledAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task Step0_NextButtonEnabled_AfterFillingRequiredFields()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        await createPage.FillBasicInfoAsync("Test Title", "Test Description");

        (await createPage.NextButton.IsDisabledAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task Step1_CanSelectAndDeselectTechnologies()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();
        await createPage.FillBasicInfoAsync("Test", "Test Description");
        await createPage.GoToNextStepAsync();

        // Select C#
        await createPage.SelectTechAsync("C#");
        var selectedText = Page.Locator("text=/1 selected:/");
        (await selectedText.IsVisibleAsync()).ShouldBeTrue();

        // Deselect C#
        await createPage.SelectTechAsync("C#");
        (await selectedText.IsVisibleAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task Step2_CanAddAndRemoveTags()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();
        await createPage.FillBasicInfoAsync("Test", "Test Description");
        await createPage.GoToNextStepAsync();
        await createPage.GoToNextStepAsync();

        // Add tags
        await createPage.AddTagAsync("test-tag");
        (await createPage.TagChips.CountAsync()).ShouldBe(1);

        await createPage.AddTagAsync("another-tag");
        (await createPage.TagChips.CountAsync()).ShouldBe(2);

        // Remove first tag by clicking close button
        var closeButton = createPage.TagChips.First.Locator("button").First;
        await closeButton.ClickAsync();
        (await createPage.TagChips.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Step2_AddTagViaEnterKey()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();
        await createPage.FillBasicInfoAsync("Test", "Test Description");
        await createPage.GoToNextStepAsync();
        await createPage.GoToNextStepAsync();

        await createPage.TagInput.FillAsync("enter-tag");
        await createPage.TagInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(300);

        (await createPage.TagChips.CountAsync()).ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task NavigateBetweenSteps_PreviousAndNext()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        // Previous should be disabled on step 0
        (await createPage.PreviousButton.IsDisabledAsync()).ShouldBeTrue();

        // Fill required and go next
        await createPage.FillBasicInfoAsync("Test", "Test Description");
        await createPage.GoToNextStepAsync();

        // Should be on step 1 (Technologies)
        var techHeading = Page.GetByText("Programming Languages");
        (await techHeading.IsVisibleAsync()).ShouldBeTrue();

        // Previous should now be enabled
        (await createPage.PreviousButton.IsDisabledAsync()).ShouldBeFalse();

        // Go back
        await createPage.GoToPreviousStepAsync();

        // Should be back on step 0
        var basicInfoHeading = Page.GetByText("Basic Information");
        (await basicInfoHeading.IsVisibleAsync()).ShouldBeTrue();
    }
}
