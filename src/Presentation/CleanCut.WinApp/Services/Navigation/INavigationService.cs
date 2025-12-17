namespace CleanCut.WinApp.Services.Navigation;

public interface INavigationService
{
    void ShowCustomerManagement(Form mdiParent);
    void ShowProductManagement(Form mdiParent);
    void CloseAllChildren(Form mdiParent);
}
