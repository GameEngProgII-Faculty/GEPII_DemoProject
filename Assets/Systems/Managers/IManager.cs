using System.Threading.Tasks;

public interface IManager
{
    string Name { get; }
    Task<bool> InitializeAsync();   // ← return success/failure
}
