using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Domain.Services.Models;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class MotivationService(
    IHabitRepository habitRepository,
    IAnalyticsService analyticsService,
    IRecurrenceService recurrenceService,
    IDateService dateService) : IMotivationService
{
    private readonly IHabitRepository _habitRepository = habitRepository;
    private readonly IAnalyticsService _analyticsService = analyticsService;
    private readonly IRecurrenceService _recurrenceService = recurrenceService;
    private readonly IDateService _dateService = dateService;

    public async Task<MotivationSummary> GetSummaryAsync(int userId)
    {
        var overview = await _analyticsService.GetOverviewAsync(userId);
        var trends = await _analyticsService.GetTrendsAsync(userId);
        var streakCenter = await GetStreakCenterAsync(userId);
        var achievements = await GetAchievementsAsync(userId);
        var challenges = await GetMonthlyChallengesAsync(userId);
        var insights = BuildMotivationalInsights(
            overview,
            trends,
            streakCenter.HabitsAtRisk,
            achievements,
            challenges);

        return new MotivationSummary(
            userId,
            _dateService.Today,
            CalculateConsistencyScore(overview, trends),
            overview.CurrentOverallStreak,
            overview.LongestOverallStreak,
            achievements.UnlockedCount,
            achievements.TotalCount,
            challenges.Challenges.Count(challenge => !challenge.IsCompleted),
            streakCenter.HabitsAtRisk.FirstOrDefault(),
            insights);
    }

    public async Task<StreakCenter> GetStreakCenterAsync(int userId)
    {
        var today = _dateService.Today;
        var overview = await _analyticsService.GetOverviewAsync(userId);
        var trends = await _analyticsService.GetTrendsAsync(userId);
        var activeHabits = await GetActiveHabitsAsync(userId);
        var habitAnalytics = await GetHabitAnalyticsAsync(userId, activeHabits);
        var risks = BuildHabitsAtRisk(activeHabits, habitAnalytics, today);
        var streaks = habitAnalytics
            .OrderByDescending(habit => habit.CurrentStreak)
            .ThenByDescending(habit => habit.LongestStreak)
            .ThenBy(habit => habit.Title)
            .Select(habit => ToHabitStreakStatus(habit, risks))
            .ToList();

        return new StreakCenter(
            userId,
            today,
            CalculateConsistencyScore(overview, trends),
            overview.CurrentOverallStreak,
            overview.LongestOverallStreak,
            streaks,
            risks,
            BuildStreakInsights(overview, trends, risks, streaks));
    }

    public async Task<AchievementSet> GetAchievementsAsync(int userId)
    {
        var overview = await _analyticsService.GetOverviewAsync(userId);
        var trends = await _analyticsService.GetTrendsAsync(userId);
        var calendar = await _analyticsService.GetCalendarAsync(userId);
        var activeHabits = await GetActiveHabitsAsync(userId);
        var habitAnalytics = await GetHabitAnalyticsAsync(userId, activeHabits);
        var consistencyScore = CalculateConsistencyScore(overview, trends);
        var achievements = BuildAchievements(
            overview,
            trends,
            calendar,
            habitAnalytics,
            consistencyScore);

        return new AchievementSet(
            userId,
            _dateService.Today,
            consistencyScore,
            achievements.Count(achievement => achievement.IsUnlocked),
            achievements.Count,
            achievements);
    }

    public async Task<MonthlyChallengeSet> GetMonthlyChallengesAsync(int userId)
    {
        var today = _dateService.Today;
        var startDate = new DateOnly(today.Year, today.Month, 1);
        var overview = await _analyticsService.GetOverviewAsync(userId);
        var calendar = await _analyticsService.GetCalendarAsync(userId);
        var activeHabits = await GetActiveHabitsAsync(userId);
        var habitAnalytics = await GetHabitAnalyticsAsync(userId, activeHabits);
        var monthDays = calendar.Days
            .Where(day => day.Date >= startDate && day.Date <= today)
            .ToList();
        var scheduledThisMonth = monthDays.Sum(day => day.ScheduledCount);
        var completedThisMonth = monthDays.Sum(day => day.CompletedCount);
        var monthCompletionRate = scheduledThisMonth == 0
            ? 0
            : Percentage(completedThisMonth, scheduledThisMonth);
        var perfectDays = monthDays.Count(day => day.Status == "perfect");
        var protectedHabits = habitAnalytics.Count(habit => habit.CurrentStreak > 0);
        var completionTarget = Math.Max(12, Math.Min(40, scheduledThisMonth == 0 ? 20 : scheduledThisMonth));
        var challenges = new List<MonthlyChallenge>
        {
            CreateChallenge(
                "monthly-consistency",
                "Monthly Consistency",
                "Reach an 85% completion rate on scheduled habits this month.",
                "calendar_month",
                monthCompletionRate,
                85,
                "Keep the month steady."),
            CreateChallenge(
                "perfect-days",
                "Perfect Days",
                "Finish every scheduled habit on 10 days this month.",
                "verified",
                perfectDays,
                10,
                "Stack more perfect days."),
            CreateChallenge(
                "completion-volume",
                "Completion Volume",
                $"Complete {completionTarget} scheduled habit checks this month.",
                "done_all",
                completedThisMonth,
                completionTarget,
                "Each scheduled check moves this forward."),
            CreateChallenge(
                "protect-streaks",
                "Protect Your Streaks",
                "Keep every active habit on a live streak.",
                "local_fire_department",
                protectedHabits,
                Math.Max(1, overview.TotalActiveHabits),
                "Bring every habit back onto a streak.")
        };

        return new MonthlyChallengeSet(
            userId,
            today.ToString("MMMM yyyy"),
            startDate,
            today,
            challenges);
    }

    private async Task<List<Habit>> GetActiveHabitsAsync(int userId)
        => (await _habitRepository.GetByUserIdWithCompletionsAsync(userId, activeOnly: true))
            .OrderBy(habit => habit.Title)
            .ToList();

    private async Task<IReadOnlyCollection<HabitAnalytics>> GetHabitAnalyticsAsync(
        int userId,
        IReadOnlyCollection<Habit> habits)
    {
        var analytics = new List<HabitAnalytics>();

        foreach (var habit in habits)
        {
            analytics.Add(await _analyticsService.GetHabitAnalyticsAsync(userId, habit.Id));
        }

        return analytics;
    }

    private IReadOnlyCollection<MotivationHabitAtRisk> BuildHabitsAtRisk(
        IReadOnlyCollection<Habit> activeHabits,
        IReadOnlyCollection<HabitAnalytics> habitAnalytics,
        DateOnly today)
    {
        var habitsById = activeHabits.ToDictionary(habit => habit.Id);

        return habitAnalytics
            .Select(habit => BuildHabitRisk(habitsById[habit.HabitId], habit, today))
            .Where(risk => risk is not null)
            .Select(risk => risk!)
            .OrderByDescending(risk => RiskWeight(risk.RiskLevel))
            .ThenByDescending(risk => risk.MissedScheduledDatesCount)
            .ThenBy(risk => risk.Title)
            .ToList();
    }

    private MotivationHabitAtRisk? BuildHabitRisk(Habit habit, HabitAnalytics analytics, DateOnly today)
    {
        var missedThisWeek = analytics.WeeklyTrend.Sum(day =>
            Math.Max(0, day.ScheduledHabits - day.CompletedHabits));
        var isScheduledToday = _recurrenceService.IsHabitScheduledForDate(habit, today);
        var isMissedToday = analytics.MissedScheduledDates.Contains(today);
        var completionRate = analytics.CompletionRate;

        if (!isMissedToday && missedThisWeek == 0 && completionRate >= 60)
        {
            return null;
        }

        var riskLevel = isMissedToday || missedThisWeek >= 2 || completionRate < 40
            ? "high"
            : "medium";
        var message = isScheduledToday && isMissedToday
            ? $"{analytics.Title} is scheduled today and still open."
            : completionRate < 40
                ? $"{analytics.Title} is below 40% consistency."
                : $"{analytics.Title} has missed scheduled days this week.";

        return new MotivationHabitAtRisk(
            analytics.HabitId,
            analytics.Title,
            analytics.Icon,
            analytics.Color,
            analytics.Category,
            analytics.CurrentStreak,
            analytics.LastCompletedDate,
            FindNextScheduledDate(habit, today),
            missedThisWeek,
            riskLevel,
            message);
    }

    private DateOnly? FindNextScheduledDate(Habit habit, DateOnly today)
    {
        for (var date = today; date <= today.AddDays(14); date = date.AddDays(1))
        {
            if (_recurrenceService.IsHabitScheduledForDate(habit, date))
            {
                return date;
            }
        }

        return null;
    }

    private HabitStreakStatus ToHabitStreakStatus(
        HabitAnalytics habit,
        IReadOnlyCollection<MotivationHabitAtRisk> risks)
    {
        var risk = risks.FirstOrDefault(item => item.HabitId == habit.HabitId);

        if (habit.TotalCompletions == 0)
        {
            return new HabitStreakStatus(
                habit.HabitId,
                habit.Title,
                habit.Icon,
                habit.Color,
                habit.Category,
                habit.CurrentStreak,
                habit.LongestStreak,
                habit.CompletionRate,
                habit.TotalCompletions,
                habit.LastCompletedDate,
                "new",
                "Complete this habit to start a streak.");
        }

        if (risk is not null)
        {
            return new HabitStreakStatus(
                habit.HabitId,
                habit.Title,
                habit.Icon,
                habit.Color,
                habit.Category,
                habit.CurrentStreak,
                habit.LongestStreak,
                habit.CompletionRate,
                habit.TotalCompletions,
                habit.LastCompletedDate,
                "at-risk",
                risk.Message);
        }

        if (habit.CurrentStreak > 0)
        {
            return new HabitStreakStatus(
                habit.HabitId,
                habit.Title,
                habit.Icon,
                habit.Color,
                habit.Category,
                habit.CurrentStreak,
                habit.LongestStreak,
                habit.CompletionRate,
                habit.TotalCompletions,
                habit.LastCompletedDate,
                "protected",
                $"{habit.Title} is protected today.");
        }

        return new HabitStreakStatus(
            habit.HabitId,
            habit.Title,
            habit.Icon,
            habit.Color,
            habit.Category,
            habit.CurrentStreak,
            habit.LongestStreak,
            habit.CompletionRate,
            habit.TotalCompletions,
            habit.LastCompletedDate,
            "rebuilding",
            $"Restart {habit.Title} with the next scheduled completion.");
    }

    private static IReadOnlyCollection<AchievementProgress> BuildAchievements(
        AnalyticsOverview overview,
        TrendAnalytics trends,
        CalendarAnalytics calendar,
        IReadOnlyCollection<HabitAnalytics> habitAnalytics,
        int consistencyScore)
    {
        var perfectStreak = CalculateLongestPerfectDayStreak(calendar.Days);
        var protectedHabits = habitAnalytics.Count(habit => habit.CurrentStreak >= 3);

        return
        [
            CreateAchievement(
                "first-check",
                "First Check",
                "Complete your first scheduled habit.",
                "check_circle",
                "Foundation",
                overview.TotalCompletions,
                1,
                "One check starts the system."),
            CreateAchievement(
                "ten-checks",
                "Ten Checks",
                "Reach 10 total habit completions.",
                "done_all",
                "Foundation",
                overview.TotalCompletions,
                10,
                "Build a visible base."),
            CreateAchievement(
                "hundred-checks",
                "Hundred Checks",
                "Reach 100 total habit completions.",
                "workspace_premium",
                "Milestone",
                overview.TotalCompletions,
                100,
                "Long-term progress is compounding."),
            CreateAchievement(
                "week-streak",
                "Seven-Day Streak",
                "Maintain a perfect overall streak for 7 scheduled days.",
                "local_fire_department",
                "Streak",
                overview.LongestOverallStreak,
                7,
                "Protect every scheduled day for a week."),
            CreateAchievement(
                "month-streak",
                "Thirty-Day Streak",
                "Maintain a perfect overall streak for 30 scheduled days.",
                "bolt",
                "Streak",
                overview.LongestOverallStreak,
                30,
                "A full month of protected habits."),
            CreateAchievement(
                "consistent",
                "Reliable Rhythm",
                "Reach a 70% recurrence-aware completion rate.",
                "trending_up",
                "Consistency",
                overview.AverageCompletionRate,
                70,
                "Keep scheduled habits above 70%."),
            CreateAchievement(
                "elite-consistency",
                "Elite Consistency",
                "Reach a 90% recurrence-aware completion rate.",
                "stars",
                "Consistency",
                overview.AverageCompletionRate,
                90,
                "A premium rhythm across scheduled days."),
            CreateAchievement(
                "perfect-week",
                "Perfect Week",
                "Complete every scheduled habit for 7 scheduled days in a row.",
                "verified",
                "Consistency",
                perfectStreak,
                7,
                "Stack seven perfect scheduled days."),
            CreateAchievement(
                "flow-builder",
                "Flow Builder",
                "Keep 3 active habits on streaks of at least 3 days.",
                "account_tree",
                "Habit Mix",
                protectedHabits,
                3,
                "Create consistency across multiple habits."),
            CreateAchievement(
                "strong-week",
                "Strong Week",
                "Reach an 80% completion rate in the last 7 days.",
                "insights",
                "Momentum",
                trends.Last7Days.CompletionRate,
                80,
                "Finish the week above 80%."),
            CreateAchievement(
                "consistency-score",
                "Consistency Score",
                "Reach an 85 motivation consistency score.",
                "speed",
                "Motivation",
                consistencyScore,
                85,
                "Balance completion rate, weekly rhythm and streak.")
        ];
    }

    private static AchievementProgress CreateAchievement(
        string id,
        string title,
        string description,
        string icon,
        string category,
        int currentValue,
        int targetValue,
        string lockedMessage)
    {
        var progressPercent = ProgressPercent(currentValue, targetValue);
        var isUnlocked = currentValue >= targetValue;

        return new AchievementProgress(
            id,
            title,
            description,
            icon,
            category,
            Math.Min(currentValue, targetValue),
            targetValue,
            progressPercent,
            isUnlocked,
            isUnlocked ? "Unlocked" : lockedMessage);
    }

    private static MonthlyChallenge CreateChallenge(
        string id,
        string title,
        string description,
        string icon,
        int currentValue,
        int targetValue,
        string activeMessage)
    {
        var progressPercent = ProgressPercent(currentValue, targetValue);
        var isCompleted = currentValue >= targetValue;

        return new MonthlyChallenge(
            id,
            title,
            description,
            icon,
            Math.Min(currentValue, targetValue),
            targetValue,
            progressPercent,
            isCompleted,
            isCompleted ? "Completed" : activeMessage);
    }

    private static IReadOnlyCollection<string> BuildMotivationalInsights(
        AnalyticsOverview overview,
        TrendAnalytics trends,
        IReadOnlyCollection<MotivationHabitAtRisk> risks,
        AchievementSet achievements,
        MonthlyChallengeSet challenges)
    {
        var insights = new List<string>();

        if (overview.CurrentOverallStreak > 0)
        {
            insights.Add($"Your overall streak is {overview.CurrentOverallStreak} scheduled days.");
        }

        if (trends.Last7Days.CompletionRate > trends.Last30Days.CompletionRate)
        {
            insights.Add("Your rhythm is improving this week.");
        }

        if (risks.Count > 0)
        {
            insights.Add($"{risks.First().Title} needs attention today.");
        }

        if (achievements.UnlockedCount > 0)
        {
            insights.Add($"{achievements.UnlockedCount} achievements are already unlocked.");
        }

        var closestChallenge = challenges.Challenges
            .Where(challenge => !challenge.IsCompleted)
            .OrderByDescending(challenge => challenge.ProgressPercent)
            .FirstOrDefault();

        if (closestChallenge is not null)
        {
            insights.Add($"{closestChallenge.Title} is {closestChallenge.ProgressPercent}% complete.");
        }

        if (insights.Count == 0)
        {
            insights.Add("Complete a few scheduled habits to activate your motivation system.");
        }

        return insights.Take(4).ToList();
    }

    private static IReadOnlyCollection<string> BuildStreakInsights(
        AnalyticsOverview overview,
        TrendAnalytics trends,
        IReadOnlyCollection<MotivationHabitAtRisk> risks,
        IReadOnlyCollection<HabitStreakStatus> streaks)
    {
        var insights = new List<string>();

        if (overview.CurrentOverallStreak > 0)
        {
            insights.Add($"You are on a {overview.CurrentOverallStreak}-day overall streak.");
        }

        var strongest = streaks
            .Where(streak => streak.CurrentStreak > 0)
            .OrderByDescending(streak => streak.CurrentStreak)
            .FirstOrDefault();

        if (strongest is not null)
        {
            insights.Add($"{strongest.Title} has your strongest live streak.");
        }

        if (risks.Count > 0)
        {
            insights.Add($"{risks.Count} habit{(risks.Count == 1 ? " is" : "s are")} at risk.");
        }

        if (trends.Last7Days.CompletionRate >= 80)
        {
            insights.Add("This week is keeping your streak foundation strong.");
        }

        if (insights.Count == 0)
        {
            insights.Add("Complete a scheduled habit to begin a new streak.");
        }

        return insights.Take(4).ToList();
    }

    private static int CalculateConsistencyScore(AnalyticsOverview overview, TrendAnalytics trends)
    {
        var streakBonus = Math.Min(10, overview.CurrentOverallStreak);
        var weightedScore = (overview.AverageCompletionRate * 0.6m)
            + (trends.Last7Days.CompletionRate * 0.3m)
            + streakBonus;

        return Math.Clamp((int)Math.Round(weightedScore, MidpointRounding.AwayFromZero), 0, 100);
    }

    private static int CalculateLongestPerfectDayStreak(IReadOnlyCollection<CalendarAnalyticsDay> days)
    {
        var longest = 0;
        var current = 0;

        foreach (var day in days.OrderBy(day => day.Date))
        {
            if (day.Status == "none")
            {
                continue;
            }

            if (day.Status == "perfect")
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 0;
            }
        }

        return longest;
    }

    private static int ProgressPercent(int currentValue, int targetValue)
        => targetValue <= 0 ? 0 : Math.Clamp(Percentage(currentValue, targetValue), 0, 100);

    private static int Percentage(int value, int total)
        => total == 0 ? 0 : (int)Math.Round(value * 100m / total, MidpointRounding.AwayFromZero);

    private static int RiskWeight(string riskLevel)
        => riskLevel == "high" ? 2 : 1;
}
