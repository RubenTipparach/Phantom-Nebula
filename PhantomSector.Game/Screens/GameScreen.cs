using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhantomSector.Game.Screens;

public enum ScreenState
{
    TransitionOn,
    Active,
    TransitionOff,
    Hidden
}

public abstract class GameScreen
{
    public string Name { get; protected set; }
    public ScreenState ScreenState { get; protected set; } = ScreenState.TransitionOn;
    public bool IsPopup { get; protected set; }
    public bool IsExiting { get; protected set; }

    public float TransitionPosition { get; protected set; }
    public float TransitionOnTime { get; protected set; } = 0.5f;
    public float TransitionOffTime { get; protected set; } = 0.5f;
    public float TransitionAlpha => 1f - TransitionPosition;

    protected ScreenManager ScreenManager { get; private set; }
    protected Game1 Game => ScreenManager.Game;

    public bool IsActive => !IsExiting && (ScreenState == ScreenState.TransitionOn || ScreenState == ScreenState.Active);

    protected GameScreen(string name)
    {
        Name = name;
    }

    public virtual void LoadContent() { }

    public virtual void UnloadContent() { }

    public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        if (IsExiting)
        {
            ScreenState = ScreenState.TransitionOff;
            if (!UpdateTransition(gameTime, TransitionOffTime, 1))
            {
                ScreenManager.RemoveScreen(this);
            }
        }
        else if (coveredByOtherScreen)
        {
            if (UpdateTransition(gameTime, TransitionOffTime, 1))
            {
                ScreenState = ScreenState.TransitionOff;
            }
            else
            {
                ScreenState = ScreenState.Hidden;
            }
        }
        else
        {
            if (UpdateTransition(gameTime, TransitionOnTime, -1))
            {
                ScreenState = ScreenState.TransitionOn;
            }
            else
            {
                ScreenState = ScreenState.Active;
            }
        }
    }

    private bool UpdateTransition(GameTime gameTime, float time, int direction)
    {
        float transitionDelta;

        if (time == 0)
            transitionDelta = 1;
        else
            transitionDelta = (float)(gameTime.ElapsedGameTime.TotalSeconds / time);

        TransitionPosition += transitionDelta * direction;

        if ((direction < 0 && TransitionPosition <= 0) || (direction > 0 && TransitionPosition >= 1))
        {
            TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
            return false;
        }

        return true;
    }

    public virtual void HandleInput(GameTime gameTime) { }

    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    public void ExitScreen()
    {
        if (TransitionOffTime == 0)
        {
            ScreenManager.RemoveScreen(this);
        }
        else
        {
            IsExiting = true;
        }
    }

    internal void SetScreenManager(ScreenManager screenManager)
    {
        ScreenManager = screenManager;
    }
}
