namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class VintageComposerService 
{
    private IVintageComposer fredClientComposer;
    
    public VintageComposerService(IVintageComposer fredClientComposer) => this.fredClientComposer = fredClientComposer;
    

    public List<IFredObservation> MakeDense(List<IFredObservation> sparse) => fredClientComposer.MakeDense(sparse);


    public List<IFredVintage> MakeDense(List<IFredVintage> sparseVintages) => fredClientComposer.MakeDense(sparseVintages);
    

    public List<IFredObservation> MakeSparse(List<IFredObservation> dense) => fredClientComposer.MakeSparse(dense);
}
