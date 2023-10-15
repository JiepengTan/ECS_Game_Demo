using System;
using GamesTan.ECS.Game;
using UnityEngine;

// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using Lockstep.Game;
using UnityEngine;

public abstract class BaseMainScript : MonoBehaviour {
    public Launcher launcher = new Launcher();
    public bool IsVideoMode = false;

    protected ServiceContainer _serviceContainer;
    public bool HasInit { get; private set; }

    protected virtual void Awake(){
        HasInit = true;
        Debug.Log(Application.persistentDataPath);
        Lockstep.Logging.Logger.OnMessage += UnityLogHandler.OnLog;
        _serviceContainer = CreateServiceContainer();
        _serviceContainer.GetService<ISimulatorService>().FuncCreateWorld = CreateWorld;
        DoAwake();
        launcher.DoAwake(_serviceContainer);
    }

    protected virtual ServiceContainer CreateServiceContainer() {
        throw new NotImplementedException("CreateServiceContainer");
    }

    protected virtual object CreateWorld(IServiceContainer services) {
        throw new NotImplementedException("CreateWorld");
    }

    protected virtual void Start(){
        DoStart();
        launcher.DoStart();
    }

    private void Update(){
        DoUpdate();
        launcher.DoUpdate(Time.deltaTime);
    }

    private void OnDestroy(){
        launcher.DoDestroy();
        DoDestroy();
    }

    private void OnApplicationQuit(){
        launcher.OnApplicationQuit();
    }

    public T GetService<T>() where T : IService{
        return _serviceContainer.GetService<T>();
    }

    protected virtual void DoAwake() {
    }
    protected virtual void DoStart() {
    }

    protected virtual void DoUpdate() {
    }
    protected virtual void DoDestroy() {
    }

}