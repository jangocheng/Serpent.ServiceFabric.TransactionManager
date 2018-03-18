namespace TransactionManager
{
    using Microsoft.ServiceFabric.Actors;

    internal interface IDeleteActor
    {
        void DeleteActor(ActorId actorId);
    }
}