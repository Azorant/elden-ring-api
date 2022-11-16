namespace EldenRingAPI.Interfaces
{
    public interface IAPIService
    {
        Task<Response> getWeapons(int page = 0);
        Task<Response> getWeapon(string id);
    }
}
