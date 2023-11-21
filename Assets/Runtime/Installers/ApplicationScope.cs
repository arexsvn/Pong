using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ApplicationScope : LifetimeScope
{
    [SerializeField] SpriteCollection spriteCollection;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<AppController>();

        builder.Register<UICreator>(Lifetime.Singleton);
        builder.Register<UIController>(Lifetime.Singleton);
        builder.Register<NetworkGameplayManager>(Lifetime.Singleton);
        builder.Register<LocalGameplayManager>(Lifetime.Singleton);
        builder.Register<MainMenuController>(Lifetime.Singleton);
        builder.Register<HudController>(Lifetime.Singleton);
        builder.Register<LocaleManager>(Lifetime.Singleton);
        builder.Register<SaveStateController>(Lifetime.Singleton);
        builder.Register<AudioController>(Lifetime.Singleton);
        builder.Register<ParticleController>(Lifetime.Singleton);
        builder.Register<SettingsController>(Lifetime.Singleton);
        builder.Register<VisualThemeController>(Lifetime.Singleton);
        builder.Register<GameController>(Lifetime.Singleton);
        builder.Register<CourtController>(Lifetime.Singleton);

        builder.RegisterInstance(spriteCollection);

        new DynamicGameObjectInstaller().Install(builder);
    }
}