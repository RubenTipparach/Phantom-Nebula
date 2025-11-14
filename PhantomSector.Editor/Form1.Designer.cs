namespace PhantomSector.Editor;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.gameViewControl = new PhantomSector.Editor.GameViewControl();
        this.panelControls = new System.Windows.Forms.Panel();
        this.btnRotateLeft = new System.Windows.Forms.Button();
        this.btnRotateRight = new System.Windows.Forms.Button();
        this.btnRotateUp = new System.Windows.Forms.Button();
        this.btnRotateDown = new System.Windows.Forms.Button();
        this.btnRotateCW = new System.Windows.Forms.Button();
        this.btnRotateCCW = new System.Windows.Forms.Button();
        this.btnReset = new System.Windows.Forms.Button();
        this.lblRotation = new System.Windows.Forms.Label();
        this.panelControls.SuspendLayout();
        this.SuspendLayout();
        //
        // gameViewControl
        //
        this.gameViewControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.gameViewControl.Location = new System.Drawing.Point(12, 12);
        this.gameViewControl.MouseHoverUpdatesOnly = false;
        this.gameViewControl.Name = "gameViewControl";
        this.gameViewControl.Size = new System.Drawing.Size(776, 300);
        this.gameViewControl.TabIndex = 0;
        this.gameViewControl.Text = "gameViewControl";
        //
        // panelControls
        //
        this.panelControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.panelControls.Controls.Add(this.lblRotation);
        this.panelControls.Controls.Add(this.btnReset);
        this.panelControls.Controls.Add(this.btnRotateCCW);
        this.panelControls.Controls.Add(this.btnRotateCW);
        this.panelControls.Controls.Add(this.btnRotateDown);
        this.panelControls.Controls.Add(this.btnRotateUp);
        this.panelControls.Controls.Add(this.btnRotateRight);
        this.panelControls.Controls.Add(this.btnRotateLeft);
        this.panelControls.Location = new System.Drawing.Point(12, 318);
        this.panelControls.Name = "panelControls";
        this.panelControls.Size = new System.Drawing.Size(776, 120);
        this.panelControls.TabIndex = 1;
        //
        // btnRotateLeft
        //
        this.btnRotateLeft.Location = new System.Drawing.Point(15, 40);
        this.btnRotateLeft.Name = "btnRotateLeft";
        this.btnRotateLeft.Size = new System.Drawing.Size(100, 30);
        this.btnRotateLeft.TabIndex = 0;
        this.btnRotateLeft.Text = "Rotate Left (Y-)";
        this.btnRotateLeft.UseVisualStyleBackColor = true;
        this.btnRotateLeft.Click += new System.EventHandler(this.BtnRotateLeft_Click);
        //
        // btnRotateRight
        //
        this.btnRotateRight.Location = new System.Drawing.Point(121, 40);
        this.btnRotateRight.Name = "btnRotateRight";
        this.btnRotateRight.Size = new System.Drawing.Size(100, 30);
        this.btnRotateRight.TabIndex = 1;
        this.btnRotateRight.Text = "Rotate Right (Y+)";
        this.btnRotateRight.UseVisualStyleBackColor = true;
        this.btnRotateRight.Click += new System.EventHandler(this.BtnRotateRight_Click);
        //
        // btnRotateUp
        //
        this.btnRotateUp.Location = new System.Drawing.Point(227, 40);
        this.btnRotateUp.Name = "btnRotateUp";
        this.btnRotateUp.Size = new System.Drawing.Size(100, 30);
        this.btnRotateUp.TabIndex = 2;
        this.btnRotateUp.Text = "Rotate Up (X-)";
        this.btnRotateUp.UseVisualStyleBackColor = true;
        this.btnRotateUp.Click += new System.EventHandler(this.BtnRotateUp_Click);
        //
        // btnRotateDown
        //
        this.btnRotateDown.Location = new System.Drawing.Point(333, 40);
        this.btnRotateDown.Name = "btnRotateDown";
        this.btnRotateDown.Size = new System.Drawing.Size(100, 30);
        this.btnRotateDown.TabIndex = 3;
        this.btnRotateDown.Text = "Rotate Down (X+)";
        this.btnRotateDown.UseVisualStyleBackColor = true;
        this.btnRotateDown.Click += new System.EventHandler(this.BtnRotateDown_Click);
        //
        // btnRotateCW
        //
        this.btnRotateCW.Location = new System.Drawing.Point(439, 40);
        this.btnRotateCW.Name = "btnRotateCW";
        this.btnRotateCW.Size = new System.Drawing.Size(100, 30);
        this.btnRotateCW.TabIndex = 4;
        this.btnRotateCW.Text = "Roll CW (Z+)";
        this.btnRotateCW.UseVisualStyleBackColor = true;
        this.btnRotateCW.Click += new System.EventHandler(this.BtnRotateCW_Click);
        //
        // btnRotateCCW
        //
        this.btnRotateCCW.Location = new System.Drawing.Point(545, 40);
        this.btnRotateCCW.Name = "btnRotateCCW";
        this.btnRotateCCW.Size = new System.Drawing.Size(100, 30);
        this.btnRotateCCW.TabIndex = 5;
        this.btnRotateCCW.Text = "Roll CCW (Z-)";
        this.btnRotateCCW.UseVisualStyleBackColor = true;
        this.btnRotateCCW.Click += new System.EventHandler(this.BtnRotateCCW_Click);
        //
        // btnReset
        //
        this.btnReset.Location = new System.Drawing.Point(651, 40);
        this.btnReset.Name = "btnReset";
        this.btnReset.Size = new System.Drawing.Size(100, 30);
        this.btnReset.TabIndex = 6;
        this.btnReset.Text = "Reset";
        this.btnReset.UseVisualStyleBackColor = true;
        this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
        //
        // lblRotation
        //
        this.lblRotation.AutoSize = true;
        this.lblRotation.Location = new System.Drawing.Point(15, 10);
        this.lblRotation.Name = "lblRotation";
        this.lblRotation.Size = new System.Drawing.Size(150, 15);
        this.lblRotation.TabIndex = 7;
        this.lblRotation.Text = "Rotation: X=0° Y=0° Z=0°";
        //
        // Form1
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.panelControls);
        this.Controls.Add(this.gameViewControl);
        this.Name = "Form1";
        this.Text = "PhantomSector Editor - 3D Viewport";
        this.panelControls.ResumeLayout(false);
        this.panelControls.PerformLayout();
        this.ResumeLayout(false);
    }

    #endregion

    private GameViewControl gameViewControl;
    private System.Windows.Forms.Panel panelControls;
    private System.Windows.Forms.Button btnRotateLeft;
    private System.Windows.Forms.Button btnRotateRight;
    private System.Windows.Forms.Button btnRotateUp;
    private System.Windows.Forms.Button btnRotateDown;
    private System.Windows.Forms.Button btnRotateCW;
    private System.Windows.Forms.Button btnRotateCCW;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Label lblRotation;
}
