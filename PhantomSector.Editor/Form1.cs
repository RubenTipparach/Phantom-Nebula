namespace PhantomSector.Editor;

public partial class Form1 : Form
{
    private const float RotationStep = 15f;

    public Form1()
    {
        InitializeComponent();
    }

    private void BtnRotateLeft_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationY -= RotationStep;
        UpdateRotationLabel();
    }

    private void BtnRotateRight_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationY += RotationStep;
        UpdateRotationLabel();
    }

    private void BtnRotateUp_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationX -= RotationStep;
        UpdateRotationLabel();
    }

    private void BtnRotateDown_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationX += RotationStep;
        UpdateRotationLabel();
    }

    private void BtnRotateCW_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationZ += RotationStep;
        UpdateRotationLabel();
    }

    private void BtnRotateCCW_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationZ -= RotationStep;
        UpdateRotationLabel();
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        gameViewControl.RotationX = 0f;
        gameViewControl.RotationY = 0f;
        gameViewControl.RotationZ = 0f;
        UpdateRotationLabel();
    }

    private void UpdateRotationLabel()
    {
        lblRotation.Text = $"Rotation: X={gameViewControl.RotationX:F0}° Y={gameViewControl.RotationY:F0}° Z={gameViewControl.RotationZ:F0}°";
    }
}
