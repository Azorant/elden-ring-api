namespace EldenRingAPI.Interfaces
{
    public interface IAPIService
    {
        Task<Response> getWeapons(int page, int perPage);
        Task<Response> getWeapon(string id);
    }
}
