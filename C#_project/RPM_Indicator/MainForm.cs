using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using System.Linq;
using System.Threading;


namespace NationalInstruments.Examples.GenDigPulseTrain_Continuous
{

    public class MainForm : System.Windows.Forms.Form
    {
		GroupBox channelParameterGroupBox;
		Label physicalChannelLabel;
		TextBox frequencyTextBox;
		Label frequencyLabel;
		TextBox dutyCycleTextBox;
		Label dutyCycleLabel;
		Button startButton;
		Button stopButton;
		Task pwmTask;
		COPulseIdleState idleState;
		GroupBox idleStateGroupBox;
		RadioButton highRadioButton;
		RadioButton lowRadioButton;
		System.Windows.Forms.Timer statusCheckTimer;
		ComboBox counterComboBox;
		IContainer components;
		BackgroundWorker frequencyLogWorker;			
		AnalogSingleChannelReader reader;
		
		double[] samples;
		int valtasokSzama;
		double fanFrequency;
		double setRPM_value;
		
		double Kp;
		double Ki;
		double Kd;
		
		double error;
		double previousError;
		
		double integral;
		double derivative;

		double currentRPM_value;
		double output;
		
		// double PWM = 0.5;
		

        private double _dutyCycle = 0.5;
        
        public double dutyCycle
        {
            get 
            { 
            	return _dutyCycle; 
            }
            set
            {
                if (_dutyCycle != value)
                {
                	_dutyCycle = value;
                    RestartBackgroundWorker();
                }
            }
        }

        private double _signalFrequency = 10000.0;
    
        public double signalFrequency
        {
            get 
            {
            	return _signalFrequency; 
            }
            set
            {
                if (_signalFrequency != value)
                {
                    _signalFrequency = value;
                    RestartBackgroundWorker();
                }
            }
        }

        //static double dutyCycle2 = 0.5;
        //static double signalFrequency2 = 10000;

		private System.Windows.Forms.Button getDataButton;
		private System.Windows.Forms.Label fanFrequencyLabel;
		private System.Windows.Forms.TextBox fanFrequencyTextBox;
		private System.Windows.Forms.Button stopDataButton;
		private System.Windows.Forms.TrackBar pwmTrackBar;


        
        public MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            pwmTrackBar.Value = (int)dutyCycle;
            frequencyTextBox.Text = signalFrequency.ToString();

            idleState = COPulseIdleState.Low;
            
            counterComboBox.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.CO, PhysicalChannelAccess.External));
            if (counterComboBox.Items.Count > 0)
                counterComboBox.SelectedIndex = 0;
            
            frequencyLogWorker = new BackgroundWorker();
            frequencyLogWorker.DoWork += new DoWorkEventHandler(frequencyLogWorker_DoWork);
            frequencyLogWorker.ProgressChanged += new ProgressChangedEventHandler(frequencyLogWorker_ProgressChanged);
            frequencyLogWorker.WorkerReportsProgress=true;
            frequencyLogWorker.WorkerSupportsCancellation=true;
            
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        	this.startButton = new System.Windows.Forms.Button();
        	this.stopButton = new System.Windows.Forms.Button();
        	this.statusCheckTimer = new System.Windows.Forms.Timer(this.components);
        	this.physicalChannelLabel = new System.Windows.Forms.Label();
        	this.frequencyLabel = new System.Windows.Forms.Label();
        	this.frequencyTextBox = new System.Windows.Forms.TextBox();
        	this.dutyCycleLabel = new System.Windows.Forms.Label();
        	this.dutyCycleTextBox = new System.Windows.Forms.TextBox();
        	this.idleStateGroupBox = new System.Windows.Forms.GroupBox();
        	this.highRadioButton = new System.Windows.Forms.RadioButton();
        	this.lowRadioButton = new System.Windows.Forms.RadioButton();
        	this.counterComboBox = new System.Windows.Forms.ComboBox();
        	this.channelParameterGroupBox = new System.Windows.Forms.GroupBox();
        	this.kdTextBox = new System.Windows.Forms.TextBox();
        	this.kiTextBox = new System.Windows.Forms.TextBox();
        	this.kpTextBox = new System.Windows.Forms.TextBox();
        	this.KdLabel = new System.Windows.Forms.Label();
        	this.KiLabel = new System.Windows.Forms.Label();
        	this.KpLabel = new System.Windows.Forms.Label();
        	this.setRPMTextBox = new System.Windows.Forms.TextBox();
        	this.setRPMLabel = new System.Windows.Forms.Label();
        	this.fanFrequencyLabel = new System.Windows.Forms.Label();
        	this.fanFrequencyTextBox = new System.Windows.Forms.TextBox();
        	this.getDataButton = new System.Windows.Forms.Button();
        	this.stopDataButton = new System.Windows.Forms.Button();
        	this.pwmTrackBar = new System.Windows.Forms.TrackBar();
        	this.RPMControlTrackbar = new System.Windows.Forms.TrackBar();
        	this.idleStateGroupBox.SuspendLayout();
        	this.channelParameterGroupBox.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pwmTrackBar)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.RPMControlTrackbar)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// startButton
        	// 
        	this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.startButton.Location = new System.Drawing.Point(240, 20);
        	this.startButton.Name = "startButton";
        	this.startButton.Size = new System.Drawing.Size(96, 32);
        	this.startButton.TabIndex = 0;
        	this.startButton.Text = "Start";
        	this.startButton.Click += new System.EventHandler(this.startButton_Click);
        	// 
        	// stopButton
        	// 
        	this.stopButton.Enabled = false;
        	this.stopButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.stopButton.Location = new System.Drawing.Point(240, 60);
        	this.stopButton.Name = "stopButton";
        	this.stopButton.Size = new System.Drawing.Size(96, 32);
        	this.stopButton.TabIndex = 1;
        	this.stopButton.Text = "Stop";
        	this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
        	// 
        	// statusCheckTimer
        	// 
        	this.statusCheckTimer.Tick += new System.EventHandler(this.statusCheckTimer_Tick);
        	// 
        	// physicalChannelLabel
        	// 
        	this.physicalChannelLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.physicalChannelLabel.Location = new System.Drawing.Point(8, 25);
        	this.physicalChannelLabel.Name = "physicalChannelLabel";
        	this.physicalChannelLabel.Size = new System.Drawing.Size(72, 16);
        	this.physicalChannelLabel.TabIndex = 0;
        	this.physicalChannelLabel.Text = "Counter(s):";
        	// 
        	// frequencyLabel
        	// 
        	this.frequencyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.frequencyLabel.Location = new System.Drawing.Point(8, 55);
        	this.frequencyLabel.Name = "frequencyLabel";
        	this.frequencyLabel.Size = new System.Drawing.Size(88, 16);
        	this.frequencyLabel.TabIndex = 2;
        	this.frequencyLabel.Text = "Frequency (Hz):";
        	// 
        	// frequencyTextBox
        	// 
        	this.frequencyTextBox.Location = new System.Drawing.Point(112, 50);
        	this.frequencyTextBox.Name = "frequencyTextBox";
        	this.frequencyTextBox.Size = new System.Drawing.Size(100, 20);
        	this.frequencyTextBox.TabIndex = 3;
        	this.frequencyTextBox.Text = "10000";
        	this.frequencyTextBox.TextChanged += new System.EventHandler(this.frequencyTextBox_TextChanged);
        	// 
        	// dutyCycleLabel
        	// 
        	this.dutyCycleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.dutyCycleLabel.Location = new System.Drawing.Point(7, 195);
        	this.dutyCycleLabel.Name = "dutyCycleLabel";
        	this.dutyCycleLabel.Size = new System.Drawing.Size(72, 16);
        	this.dutyCycleLabel.TabIndex = 4;
        	this.dutyCycleLabel.Text = "Duty Cycle:";
        	// 
        	// dutyCycleTextBox
        	// 
        	this.dutyCycleTextBox.Location = new System.Drawing.Point(112, 190);
        	this.dutyCycleTextBox.Name = "dutyCycleTextBox";
        	this.dutyCycleTextBox.Size = new System.Drawing.Size(100, 20);
        	this.dutyCycleTextBox.TabIndex = 5;
        	this.dutyCycleTextBox.Text = "0.5";
        	// 
        	// idleStateGroupBox
        	// 
        	this.idleStateGroupBox.Controls.Add(this.highRadioButton);
        	this.idleStateGroupBox.Controls.Add(this.lowRadioButton);
        	this.idleStateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.idleStateGroupBox.Location = new System.Drawing.Point(8, 312);
        	this.idleStateGroupBox.Name = "idleStateGroupBox";
        	this.idleStateGroupBox.Size = new System.Drawing.Size(208, 64);
        	this.idleStateGroupBox.TabIndex = 6;
        	this.idleStateGroupBox.TabStop = false;
        	this.idleStateGroupBox.Text = "Idle State:";
        	// 
        	// highRadioButton
        	// 
        	this.highRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.highRadioButton.Location = new System.Drawing.Point(112, 24);
        	this.highRadioButton.Name = "highRadioButton";
        	this.highRadioButton.Size = new System.Drawing.Size(64, 24);
        	this.highRadioButton.TabIndex = 1;
        	this.highRadioButton.Text = "High";
        	this.highRadioButton.CheckedChanged += new System.EventHandler(this.highRadioButton_CheckedChanged);
        	// 
        	// lowRadioButton
        	// 
        	this.lowRadioButton.Checked = true;
        	this.lowRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.lowRadioButton.Location = new System.Drawing.Point(40, 24);
        	this.lowRadioButton.Name = "lowRadioButton";
        	this.lowRadioButton.Size = new System.Drawing.Size(64, 24);
        	this.lowRadioButton.TabIndex = 0;
        	this.lowRadioButton.TabStop = true;
        	this.lowRadioButton.Text = "Low";
        	this.lowRadioButton.CheckedChanged += new System.EventHandler(this.lowRadioButton_CheckedChanged);
        	// 
        	// counterComboBox
        	// 
        	this.counterComboBox.Location = new System.Drawing.Point(112, 20);
        	this.counterComboBox.Name = "counterComboBox";
        	this.counterComboBox.Size = new System.Drawing.Size(100, 21);
        	this.counterComboBox.TabIndex = 1;
        	this.counterComboBox.Text = "Dev1/ctr0";
        	// 
        	// channelParameterGroupBox
        	// 
        	this.channelParameterGroupBox.Controls.Add(this.kdTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.kiTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.kpTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.KdLabel);
        	this.channelParameterGroupBox.Controls.Add(this.KiLabel);
        	this.channelParameterGroupBox.Controls.Add(this.KpLabel);
        	this.channelParameterGroupBox.Controls.Add(this.setRPMTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.fanFrequencyTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.setRPMLabel);
        	this.channelParameterGroupBox.Controls.Add(this.fanFrequencyLabel);
        	this.channelParameterGroupBox.Controls.Add(this.counterComboBox);
        	this.channelParameterGroupBox.Controls.Add(this.idleStateGroupBox);
        	this.channelParameterGroupBox.Controls.Add(this.dutyCycleTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.dutyCycleLabel);
        	this.channelParameterGroupBox.Controls.Add(this.frequencyTextBox);
        	this.channelParameterGroupBox.Controls.Add(this.frequencyLabel);
        	this.channelParameterGroupBox.Controls.Add(this.physicalChannelLabel);
        	this.channelParameterGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.channelParameterGroupBox.Location = new System.Drawing.Point(8, 8);
        	this.channelParameterGroupBox.Name = "channelParameterGroupBox";
        	this.channelParameterGroupBox.Size = new System.Drawing.Size(224, 406);
        	this.channelParameterGroupBox.TabIndex = 2;
        	this.channelParameterGroupBox.TabStop = false;
        	this.channelParameterGroupBox.Text = "Channel Parameters:";
        	// 
        	// kdTextBox
        	// 
        	this.kdTextBox.Location = new System.Drawing.Point(112, 140);
        	this.kdTextBox.Name = "kdTextBox";
        	this.kdTextBox.Size = new System.Drawing.Size(100, 20);
        	this.kdTextBox.TabIndex = 16;
        	this.kdTextBox.Text = "0.001";
        	// 
        	// kiTextBox
        	// 
        	this.kiTextBox.Location = new System.Drawing.Point(112, 110);
        	this.kiTextBox.Name = "kiTextBox";
        	this.kiTextBox.Size = new System.Drawing.Size(100, 20);
        	this.kiTextBox.TabIndex = 15;
        	this.kiTextBox.Text = "0.0001";
        	// 
        	// kpTextBox
        	// 
        	this.kpTextBox.Location = new System.Drawing.Point(112, 80);
        	this.kpTextBox.Name = "kpTextBox";
        	this.kpTextBox.Size = new System.Drawing.Size(100, 20);
        	this.kpTextBox.TabIndex = 14;
        	this.kpTextBox.Text = "0.000001";
        	// 
        	// KdLabel
        	// 
        	this.KdLabel.Location = new System.Drawing.Point(7, 145);
        	this.KdLabel.Name = "KdLabel";
        	this.KdLabel.Size = new System.Drawing.Size(30, 20);
        	this.KdLabel.TabIndex = 13;
        	this.KdLabel.Text = "Kd:";
        	// 
        	// KiLabel
        	// 
        	this.KiLabel.Location = new System.Drawing.Point(7, 115);
        	this.KiLabel.Name = "KiLabel";
        	this.KiLabel.Size = new System.Drawing.Size(34, 33);
        	this.KiLabel.TabIndex = 12;
        	this.KiLabel.Text = "Ki:";
        	// 
        	// KpLabel
        	// 
        	this.KpLabel.Location = new System.Drawing.Point(6, 85);
        	this.KpLabel.Name = "KpLabel";
        	this.KpLabel.Size = new System.Drawing.Size(38, 24);
        	this.KpLabel.TabIndex = 11;
        	this.KpLabel.Text = "Kp:";
        	// 
        	// setRPMTextBox
        	// 
        	this.setRPMTextBox.Location = new System.Drawing.Point(112, 230);
        	this.setRPMTextBox.Name = "setRPMTextBox";
        	this.setRPMTextBox.Size = new System.Drawing.Size(100, 20);
        	this.setRPMTextBox.TabIndex = 10;
        	// 
        	// setRPMLabel
        	// 
        	this.setRPMLabel.Location = new System.Drawing.Point(6, 235);
        	this.setRPMLabel.Name = "setRPMLabel";
        	this.setRPMLabel.Size = new System.Drawing.Size(87, 23);
        	this.setRPMLabel.TabIndex = 9;
        	this.setRPMLabel.Text = "Set RPM:";
        	// 
        	// fanFrequencyLabel
        	// 
        	this.fanFrequencyLabel.Location = new System.Drawing.Point(8, 275);
        	this.fanFrequencyLabel.Name = "fanFrequencyLabel";
        	this.fanFrequencyLabel.Size = new System.Drawing.Size(106, 15);
        	this.fanFrequencyLabel.TabIndex = 8;
        	this.fanFrequencyLabel.Text = "Current RPM:";
        	// 
        	// fanFrequencyTextBox
        	// 
        	this.fanFrequencyTextBox.Location = new System.Drawing.Point(112, 270);
        	this.fanFrequencyTextBox.Name = "fanFrequencyTextBox";
        	this.fanFrequencyTextBox.Size = new System.Drawing.Size(100, 20);
        	this.fanFrequencyTextBox.TabIndex = 7;
        	// 
        	// getDataButton
        	// 
        	this.getDataButton.Location = new System.Drawing.Point(240, 115);
        	this.getDataButton.Name = "getDataButton";
        	this.getDataButton.Size = new System.Drawing.Size(96, 32);
        	this.getDataButton.TabIndex = 3;
        	this.getDataButton.Text = "Get Data";
        	this.getDataButton.UseVisualStyleBackColor = true;
        	this.getDataButton.Click += new System.EventHandler(this.GetDataButtonClick);
        	// 
        	// stopDataButton
        	// 
        	this.stopDataButton.Location = new System.Drawing.Point(240, 155);
        	this.stopDataButton.Name = "stopDataButton";
        	this.stopDataButton.Size = new System.Drawing.Size(96, 32);
        	this.stopDataButton.TabIndex = 4;
        	this.stopDataButton.Text = "Stop Data";
        	this.stopDataButton.UseVisualStyleBackColor = true;
        	this.stopDataButton.Click += new System.EventHandler(this.StopDataButtonClick);
        	// 
        	// pwmTrackBar
        	// 
        	this.pwmTrackBar.Location = new System.Drawing.Point(235, 195);
        	this.pwmTrackBar.Maximum = 100;
        	this.pwmTrackBar.Name = "pwmTrackBar";
        	this.pwmTrackBar.Size = new System.Drawing.Size(105, 45);
        	this.pwmTrackBar.TabIndex = 5;
        	// 
        	// RPMControlTrackbar
        	// 
        	this.RPMControlTrackbar.Location = new System.Drawing.Point(235, 235);
        	this.RPMControlTrackbar.Maximum = 2500;
        	this.RPMControlTrackbar.Name = "RPMControlTrackbar";
        	this.RPMControlTrackbar.Size = new System.Drawing.Size(105, 45);
        	this.RPMControlTrackbar.TabIndex = 6;
        	this.RPMControlTrackbar.Scroll += new System.EventHandler(this.RPMControlTrackbarScroll);
        	// 
        	// MainForm
        	// 
        	this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        	this.ClientSize = new System.Drawing.Size(367, 427);
        	this.Controls.Add(this.RPMControlTrackbar);
        	this.Controls.Add(this.pwmTrackBar);
        	this.Controls.Add(this.stopDataButton);
        	this.Controls.Add(this.getDataButton);
        	this.Controls.Add(this.stopButton);
        	this.Controls.Add(this.startButton);
        	this.Controls.Add(this.channelParameterGroupBox);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.MaximizeBox = false;
        	this.Name = "MainForm";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "Generate Continuous Digital Pulse Train";
        	this.idleStateGroupBox.ResumeLayout(false);
        	this.channelParameterGroupBox.ResumeLayout(false);
        	this.channelParameterGroupBox.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pwmTrackBar)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.RPMControlTrackbar)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        [STAThread]
        static void Main() 
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new MainForm());          
        }

        public COChannel pwmChannel;
        private void startButton_Click(object sender, System.EventArgs e)
        {
        	double d=Convert.ToDouble(dutyCycleTextBox.Text);
            try
            {
                pwmTask = new Task();

               
                pwmChannel= pwmTask.COChannels.CreatePulseChannelFrequency(counterComboBox.Text, 
                    "ContinuousPulseTrain", COPulseFrequencyUnits.Hertz, idleState, 0.0, 
                    signalFrequency, 
                    d);

                pwmTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples, 1000);
                                                
                pwmTask.Start();

               
                startButton.Enabled = false;
                stopButton.Enabled = true;
                getDataButton.Enabled = true;
                stopDataButton.Enabled = false;

                statusCheckTimer.Enabled = true;
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                pwmTask.Dispose();
                startButton.Enabled = true;
                stopButton.Enabled = false;
                statusCheckTimer.Enabled = false;
            }
        }
    
        private void stopButton_Click(object sender, System.EventArgs e)
        {
            statusCheckTimer.Enabled = false;
            pwmTask.Stop();
            pwmTask.Dispose();
            startButton.Enabled = true;
            stopButton.Enabled = false;
            getDataButton.Enabled = false;
            stopDataButton.Enabled = false;
            Application.Exit();
        }
        
        void StopDataButtonClick(object sender, EventArgs e)
		{
        	stopDataButton.Enabled = false;
            getDataButton.Enabled = true;
            
            exitBW=true;
            if(frequencyLogWorker.IsBusy)
            {
            	frequencyLogWorker.CancelAsync();
            }

		}

        private void lowRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            idleState = COPulseIdleState.Low;
        }

        private void highRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            idleState = COPulseIdleState.High;
        }

        private void statusCheckTimer_Tick(object sender, System.EventArgs e)
        {
            try
            {
                if (pwmTask.IsDone)
                {
                    statusCheckTimer.Enabled = false;
                    pwmTask.Stop();
                    pwmTask.Dispose();
                    startButton.Enabled = true;
                    stopButton.Enabled = false;
                }
            }
            catch (DaqException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                statusCheckTimer.Enabled = false;
                pwmTask.Stop();
                pwmTask.Dispose();
                startButton.Enabled = true;
                stopButton.Enabled = false;
            }
        }
        
        bool exitBW=false;
        private System.Windows.Forms.TextBox setRPMTextBox;
        private System.Windows.Forms.Label setRPMLabel;
        private System.Windows.Forms.TrackBar RPMControlTrackbar;
        private System.Windows.Forms.Label KdLabel;
        private System.Windows.Forms.Label KiLabel;
        private System.Windows.Forms.Label KpLabel;
        private System.Windows.Forms.TextBox kdTextBox;
        private System.Windows.Forms.TextBox kiTextBox;
        private System.Windows.Forms.TextBox kpTextBox;
        void frequencyLogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
        	Kp = Convert.ToDouble(kpTextBox.Text);
        	Ki = Convert.ToDouble(kiTextBox.Text);
        	Kd = Convert.ToDouble(kdTextBox.Text);
        	
        	while(!exitBW) 
        	{       		
        		samples=reader.ReadMultiSample(12500);
	        	if (samples != null)
	            {
	        		double average = samples.Average();
	                double firstEdgeIndex = 0;
	                double secondEdgeIndex = 0;
	                double periodTime;
	                
	                valtasokSzama = 0;
	                for (int i = 1; i < samples.Count(); i++)
	                {
	                	if ((Math.Sign(samples[i - 1] - average) == -1) & (Math.Sign(samples[i] - average) == 1))
	                	{
	                		if (valtasokSzama % 2 == 0) 
	                		{	                		
								secondEdgeIndex = i;	                			
	                		}
	                		else
	                			firstEdgeIndex = i;
	
	                		periodTime = (secondEdgeIndex - firstEdgeIndex) * 0.000004;
	                		fanFrequency = Math.Abs(Math.Round(1/periodTime, 3));
	                		valtasokSzama++;	                	
	                	}	                        	                		                
	                }
	                
			        currentRPM_value = (60 * fanFrequency)/7;
			        
					while (currentRPM_value > 3000) 
					{
						currentRPM_value = 0;	
					}
			        
					error = setRPM_value - currentRPM_value;   
					integral = integral + error;
					derivative = error - previousError; 
					output = (Kp * error) + (Ki * integral) + (Kd * derivative);
						
					if (output > 0.99) 
					{	
						output = 0.99;	
					}
						
					if (output < 0.01) 
					{		
						output = 0.01;		
					}
						  
					previousError = error;			                
			        setPWMDuty(output);			        
			        frequencyLogWorker.ReportProgress(0);
	                
	            }
        	}
        }
        
        void frequencyLogWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	fanFrequencyTextBox.Text = Convert.ToInt32(currentRPM_value).ToString();
    		setRPMTextBox.Text = (setRPM_value).ToString();
    		dutyCycleTextBox.Text = string.Format("{0:#.##}" ,output);
    		pwmTrackBar.Value = Convert.ToInt32(output * 100);
        }       

		void GetDataButtonClick(object sender, EventArgs e)
		{
			exitBW=false;
			getDataButton.Enabled = false;
			stopDataButton.Enabled = true;
						
			Task analogInTask = new Task();
			
			AIChannel waveFrequency;
			
			waveFrequency = analogInTask.AIChannels.CreateVoltageChannel("dev1/ai1", "optoGateInput", AITerminalConfiguration.Rse, 0, 5, AIVoltageUnits.Volts);			
			analogInTask.Timing.ConfigureSampleClock("", 250000, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 12500);
			
			reader = new
				AnalogSingleChannelReader(analogInTask.Stream);
									
			frequencyLogWorker.RunWorkerAsync();
			
		}

        void RestartBackgroundWorker()
        {
            exitBW = true;
            frequencyLogWorker.CancelAsync();           
            frequencyLogWorker.RunWorkerAsync(); 
        }		
        
        private void frequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            double result = signalFrequency;
            if (double.TryParse(frequencyTextBox.Text, out result))
            {
                signalFrequency = result;
            }
            else
            {
            	MessageBox.Show(String.Format("A beírt érték nem szám: {0}!!!", frequencyTextBox.Text));
            }
        }
        
		void RPMControlTrackbarScroll(object sender, EventArgs e)
		{
			setRPM_value = RPMControlTrackbar.Value;
		}
		
		void setPWMDuty(double outDutyCycle) 
        {   		
			CounterSingleChannelWriter pwmUpdate = new CounterSingleChannelWriter(pwmTask.Stream);
			pwmUpdate.WriteSingleSample(true, new CODataFrequency(10000, outDutyCycle));			
        }

    }
}
