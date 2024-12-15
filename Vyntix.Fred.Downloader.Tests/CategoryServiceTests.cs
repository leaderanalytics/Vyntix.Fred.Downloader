namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class CategoryServiceTests : BaseTest
{
    public CategoryServiceTests(string currentProviderName) :base(currentProviderName)
    {
    }

    [Test]
    public async Task DownloadCategoryTest()
    {
        string id = "125";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadCategory(id, null));
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task DownloadCategoryChildrenTest()
    {
        string id = "13";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadCategoryChildren(id, null), EndPoint.Name);
        Assert.That(result.Success, Is.True);
        Assert.That(db.Categories.Count(x => x.ParentID == id).Equals(6));
    }

    [Test]
    public async Task DownloadRelatedCategoriesTest()
    {
        string id = "32073";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadRelatedCategories(id, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.RelatedCategories.Count(x => x.CategoryID == id).Equals(7));
    }

    [Test]
    public async Task DownloadCategorySeriesTest()
    {
        string id = "125";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadCategorySeries(id, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.SeriesCategories.Count(x => x.CategoryID == id) > 5);
    }


    [Test]
    public async Task DownloadCategoryTagsTest()
    {
        string id = "125";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadCategoryTags(id, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.CategoryTags.Count(x => x.CategoryID == id) > 20);
    }

    [Test]
    public async Task DownloadCategoriesForSeriesTest()
    {
        string symbol = "EXJPUS";
        RowOpResult result = await client.CallAsync(x => x.CategoriesService.DownloadCategoriesForSeries(symbol, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.SeriesCategories.Count(x => x.Symbol == symbol), Is.EqualTo(2));
    }
}