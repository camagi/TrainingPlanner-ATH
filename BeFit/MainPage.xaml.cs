using System.Collections.ObjectModel;
using System.Text.Json;


namespace BeFit
{

    public class Exercise
    {
        public string Name { get; set; }
        public string Sets { get; set; }
        public string Reps { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public ObservableCollection<string> WorkoutDays { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Exercise> Exercises { get; set; } = new ObservableCollection<Exercise>();

        private Dictionary<DateTime, (string plan, List<Exercise> exercises)> workoutPlans = new Dictionary<DateTime, (string, List<Exercise>)>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadWorkoutPlansFromFile(); // OdcDzP 
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            var selectedDate = e.NewDate;
            selectedDateLabel.Text = $"Wybrano datę: {selectedDate:D}";

            if (workoutPlans.TryGetValue(selectedDate, out var workoutData))
            {
                workoutPlanEntry.Text = workoutData.plan;
                Exercises.Clear();
                foreach (var exercise in workoutData.exercises)
                {
                    Exercises.Add(exercise);
                }
            }
            else
            {
                workoutPlanEntry.Text = string.Empty;
                Exercises.Clear();
            }
        }

        private void OnWorkoutDaySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is string selectedDateString)
            {
                DateTime selectedDate = DateTime.Parse(selectedDateString);

                if (workoutPlans.TryGetValue(selectedDate, out var workoutData))
                {
                    workoutPlanEntry.Text = workoutData.plan;
                    Exercises.Clear();
                    foreach (var exercise in workoutData.exercises)
                    {
                        Exercises.Add(exercise);
                    }

                    selectedDateLabel.Text = $"Wybrano datę: {selectedDate:D}";
                }
            }
        }

        private void OnAddExerciseClicked(object sender, EventArgs e)
        {
            Exercises.Add(new Exercise
            {
                Name = string.Empty,
                Sets = string.Empty,
                Reps = string.Empty
            });
        }

        private void OnAddWorkoutClicked(object sender, EventArgs e)
        {
            var selectedDate = datePicker.Date;
            var workoutPlan = workoutPlanEntry.Text;

            if (string.IsNullOrWhiteSpace(workoutPlan))
            {
                DisplayAlert("Błąd", "Proszę wpisać nazwę planu treningowego.", "OK");
                return;
            }

            var workoutExercises = new List<Exercise>(Exercises);

            if (workoutPlans.ContainsKey(selectedDate))
            {
                workoutPlans[selectedDate] = (workoutPlan, workoutExercises);
            }
            else
            {
                workoutPlans.Add(selectedDate, (workoutPlan, workoutExercises));
            }

            string dateText = selectedDate.ToString("D");
            if (!WorkoutDays.Contains(dateText))
            {
                WorkoutDays.Add(dateText);
            }

            SaveWorkoutPlansToFile();

            DisplayAlert("Sukces", $"Plan treningowy na {selectedDate:D} został dodany.", "OK");
        }

        private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "workoutPlans.json");

        private void SaveWorkoutPlansToFile()
        {
            var data = workoutPlans.Select(kv => new
            {
                Date = kv.Key,
                Plan = kv.Value.plan,
                Exercises = kv.Value.exercises
            }).ToList();

            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, json);
        }
        /// C:\...\AppData\Local\Packages\com.companyname.befit_9zz4h110yvjzm\LocalState
 
        private void LoadWorkoutPlansFromFile()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<List<WorkoutPlanData>>(json);

                if (data != null)
                {
                    workoutPlans.Clear();
                    foreach (var item in data)
                    {
                        workoutPlans[item.Date] = (item.Plan, item.Exercises);

                        string dateText = item.Date.ToString("D");
                        if (!WorkoutDays.Contains(dateText))
                        {
                            WorkoutDays.Add(dateText);
                        }
                    }
                }
            }
        }

        public class WorkoutPlanData
        {
            public DateTime Date { get; set; }
            public string Plan { get; set; }
            public List<Exercise> Exercises { get; set; }
        }
    }
}
