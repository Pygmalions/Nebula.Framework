using Nebula.Core;

namespace Nebula.ServiceModel;

/// <summary>
/// Provider is the server object to provide service.
/// </summary>
public interface IServer
{
    IService GetService();

    void Start();

    void Stop();
}