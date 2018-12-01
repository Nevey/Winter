using Game.Gameplay.Actors.Services;
using Game.Network.Data;
using Game.Network.Services;
using Game.Services;

namespace Scripts.Gameplay.Actors.Players.Services
{
    public class ClientPlayerService : Service<ClientPlayerService>
    {
        public void SpawnPlayer(SpawnData spawnData)
        {
            ClientActorService.Instance.SpawnActor(spawnData);

            if (ClientService.Instance.Client.ID == spawnData.OwningClientId)
            {
                // TODO: If controlling client id is the same is my client id, take control!
            }
        }
    }
}
