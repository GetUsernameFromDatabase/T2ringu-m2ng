using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TäringuMäng
{
    // Üritan koodiga genereerida UI, muidu liiga lihtne ülesanne
    public partial class Aken : Form
    {
        private readonly Single FontSize = 10.5F;

        private readonly Button restartButton = null;
        private readonly Player[] players = new Player[2];
        private Label result = null;

        public Aken()
        {
            var playerNames = new string[2] { "Juku", "Peeter" };
            for (int i = 0; i < 2; i++)
            {
                var player = new Player(playerNames[i], i);
                this.Controls.Add(player);
                if (restartButton == null)
                    restartButton = BeginGameButton_Create(player);
                else
                    player.Location = new Point(restartButton.Right, 0);
                players[i] = player;
            }

            result = Result_Create();
            InitializeComponent();
        }

        public void DiceRolled(Player player)
        {
            void ShowResult()
            {
                var winner = "Viik";
                var diceRolls = players.AsQueryable()
                    .Select(x => Int16.Parse(x.DiceRollSum.Text))
                    .Distinct().ToList(); // Ohtlik, kui mängijaid on rohkem kui 2
                if (diceRolls.Count > 1)
                {
                    var i = diceRolls.IndexOf(diceRolls.Max()); // IndexOfi tõttu
                    winner = "Võitis " + players[i].playerName;
                }

                result.Text = winner;
            }

            var nextPlayer = player.placement + 1;
            if (nextPlayer >= players.Length)
                ShowResult();
            else
                players[nextPlayer].Activate();
        }

        private Label Result_Create()
        {
            Label newLabel(string text, Point location)
            {
                var label = new Label
                {
                    Text = text,
                    Font = new Font(this.Font.FontFamily, this.FontSize),
                    TextAlign = ContentAlignment.TopCenter,
                    ForeColor = Color.DarkRed,
                    Location = location,
                };
                return label;
            }

            var x = 42;
            var y = restartButton.Bottom + 20;

            // "Tulemus" tekstiga labeli tegemine
            var result_label = newLabel("Tulemus", new Point(x, y));
            result_label.AutoSize = true;
            this.Controls.Add(result_label);
            // Tulemus labeli tegemine
            x = result_label.Right + 10;
            var result = newLabel("", new Point(x, y));
            result.BorderStyle = BorderStyle.Fixed3D;
            result.Margin = new Padding(20);
            this.Controls.Add(result);

            // Tulemus labeli suuruse muutmine
            /* Leiab palju tulemuse label on nihutatud player1-st
             tekitab samasuguse nihke player2-st                */
            var nihe = players[0].Right - result.Left;
            result.Width = players[1].Left - result.Left + nihe;

            return result;
        }

        private Button BeginGameButton_Create(Player firstPlayer)
        {
            var location = new Point(firstPlayer.Width,
                (int)(firstPlayer.Height * 0.9));
            var button = new Button()
            {
                Text = "Alusta uut mängu",
                Font = new Font(this.Font.FontFamily, this.FontSize),
                ForeColor = Color.White,
                BackColor = Color.OliveDrab,
                FlatStyle = FlatStyle.Popup,

                Margin = new Padding(10),
                AutoSize = true,
                Location = location,
            };
            button.Click += BeginGameButton_Click;

            this.Controls.Add(button);
            return button;
        }

        private void BeginGameButton_Click(object sender, EventArgs e)
        {
            foreach (Player player in this.players)
                player.Reset();
            players[0].Activate();
        }
    }

    public class Player : TableLayoutPanel
    {
        public readonly string playerName;
        public readonly int placement;

        public Label DiceRollSum { get; private set; }
        private TableLayoutPanel Dice { get; set; }

        public Player(string name, int placement)
        {
            this.playerName = name;
            this.placement = placement;

            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset; // Testimiseks jäetud
            this.Width = 150;
            this.Padding = new Padding(10);

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.Font = new Font(this.Font.FontFamily, 11F);
            PlayerInfo_Create();
            PointsSection_Create();
            DiceRollButton_Create();
        }

        private void AddControl(Control c)
        {
            // Oli kunagi suurem funktsioon, mis tegeles asukohaga ka
            // Avastasin "FlowLayoutPanel", millega enam asukohtadega vaeva ei pea nägema
            // Jätan alles juhuks kui pean controli lisamisel, midagi lisaks tegema
            this.Controls.Add(c);
        }

        private Label Label_Create(string text = "", Color? Colour = null)
        {
            var label = new Label
            {
                Text = text,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Colour ?? Color.DarkRed,

                Margin = new Padding(5),
                Dock = DockStyle.Fill,
                AutoSize = true,
            };
            return label;
        }

        private void PlayerInfo_Create()
        {
            var mitmes = new string[2] { "Esimene", "Teine" }[placement];
            var text = String.Format("{0} mängija\n{1}", mitmes, this.playerName);

            var identity = Label_Create(text);
            AddControl(identity);
        }

        private void PointsSection_Create()
        {
            Label DiceRollSum()  // Tekitab tulemuste kasti
            {
                var labelColour = new Color[2]
                    {Color.Green, Color.Red }[this.placement];

                var diceSum = Label_Create(Colour: labelColour);
                diceSum.BorderStyle = BorderStyle.Fixed3D;

                this.DiceRollSum = diceSum;
                return diceSum;
            }

            TableLayoutPanel DiceCreate(int rollCount = 2)  // Lisab täringu veeretuste kastid; skaleeritav
            {
                var dice = new TableLayoutPanel()
                {
                    RowCount = 1,
                    GrowStyle = TableLayoutPanelGrowStyle.AddColumns,
                    Width = 0, // Muidu laiem, kui vaja
                    Height = this.DiceRollSum.Height +
                        this.DiceRollSum.Margin.Vertical,
                    Dock = DockStyle.Fill,
                };
                var percent = 100 / rollCount; // Mitu protsenti moodustab üks täring tervest reast
                for (int i = 0; i < rollCount; i++)
                {
                    var die = Label_Create();
                    die.BorderStyle = BorderStyle.FixedSingle;
                    dice.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percent));
                    dice.Controls.Add(die);
                }
                this.Dice = dice;
                return dice;
            }

            // Lisab rohelise "Punktid"
            var pointsLabel = Label_Create("Punktid", Color.Green);

            var sectionControls = new Control[3] { pointsLabel, DiceRollSum(), DiceCreate() };
            foreach (Control c in sectionControls) { AddControl(c); }
        }

        private void DiceRollButton_Create()
        {
            var button = new Button()
            {
                Text = "Mängib " + this.playerName,
                Font = new Font(this.Font.FontFamily, this.Font.Size - 1.5F),
                BackColor = Color.SteelBlue,
                FlatStyle = FlatStyle.Popup,

                Margin = new Padding(10),
                Dock = DockStyle.Fill,
                AutoSize = true,
                Enabled = false,
            };
            button.Click += RollDice;
            AddControl(button);
        }

        private void RollDice(object sender, EventArgs e)
        {
            (sender as Button).Enabled = false;
            var rand = new Random();

            int sum = 0;
            foreach (Control die in this.Dice.Controls)
            {
                var roll = rand.Next(1, 6);
                die.Text = roll.ToString();
                sum += roll;
            }
            DiceRollSum.Text = sum.ToString();

            (this.Parent as Aken).DiceRolled(this);
        }

        public void Reset()
        {
            this.Controls[this.Controls.Count - 1].Enabled = false;
            foreach (Control die in this.Dice.Controls) { die.Text = ""; }
            DiceRollSum.Text = "";
        }

        public void Activate()
        {
            this.Controls[this.Controls.Count - 1].Enabled = true; // Nupp
        }
    }
}
