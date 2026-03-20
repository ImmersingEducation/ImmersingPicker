using ClassIsland.Shared.Enums;
using ClassIsland.Shared.IPC;
using ClassIsland.Shared.IPC.Abstractions.Services;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using ImmersingPicker.Core.Exceptions;
using Serilog;

namespace ImmersingPicker.Services.Services;

public class ClassIslandIPCService
{
    public static ClassIslandIPCService Instance = new ClassIslandIPCService();

    private IpcClient _client = new IpcClient();
    private bool _initialized = false;

    public async void Initialize()
    {
        if (_initialized) return;

        try
        {
            await _client.Connect();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ClassIsland IPC 初始化失败");
            throw new ClassIslandIPCInitializationFailed();
        }
    }

    public bool OnClass()
    {
        var lessonSc = _client.Provider.CreateIpcProxy<IPublicLessonsService>(_client.PeerProxy!);
        return lessonSc.CurrentState is TimeState.OnClass or TimeState.PrepareOnClass or TimeState.None;
    }
}