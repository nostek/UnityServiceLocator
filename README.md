# UnityServiceLocator
In Development

## Installation
```
using UnityServiceLocator;

void Awake()
{
	ServiceLocator.Register<MyGameManager>(_myGameManagerInstance);
}

void OnDestroy()
{
	ServiceLocator.Unregister<MyGameManager>();
}

void OnMyEvent()
{
	//Preferably, Get() should be cached in Awake
	ServiceLocator.Get<MyGameManager>().HandleMyEvent();
}
```

## Lookup Builder Pattern
```
using UnityServiceLocator;

void Awake()
{
	ServiceLocator
		.Get<MyGameManager>(out _myGameManagerInstance)
		.Get<MySoundManager>(out _mySoundManagerInstance)
		.TryGet<MyOptionalManager>(out _myOptionalManager)
		.Done();
}
```

## ServiceInstaller
```
using UnityServiceLocator;

ServiceInstaller serviceInstaller;

void Awake()
{
	serviceInstaller = new ServiceInstaller()
		.Register<MyGameManager>(_myGameManagerInstance)
		.Register<MySoundManager>(_mySoundManagerInstance)
		.Build();
}

void OnDestroy()
{
	//Unregister all services registered in this installer
	serviceInstaller.Dispose();
}
```
