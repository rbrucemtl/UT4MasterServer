﻿using UT4MasterServer.Services;

namespace XUnit.Tests;

public class EloTest
{
	private class Player
	{
		public double Rating { get; set; }
		public double ApproxSkillRating { get; set; }

		public Player()
		{
			Rating = 1500;
			ApproxSkillRating = 1500 + r.Next(-200, 500);
		}

		public override string ToString()
		{
			return $"Actual Skill: {ApproxSkillRating}], Current Rating: {Rating}";
		}
	}

	[Theory]
	[InlineData(100, 5, 1000)]
	private static void TestTeamEloBalancing(int gamesPerIteration, int playersPerTeam, int iterations)
	{
		int playersPerGame = playersPerTeam * 2;
		int playerCount = gamesPerIteration * playersPerGame;

		// create initial players
		Player[] players = new Player[playerCount];
		for (int i = 0; i < playerCount; i++)
		{
			players[i] = new Player();
		}

		// select different team `generations` times
		for (int i = 0; i < iterations; i++)
		{
			// randomly shuffle player indices
			var shuffledPlayerIndices = GetShuffledIndices(playerCount);
			var unshuffledPlayerIndices = GetUnshuffledIndices(shuffledPlayerIndices);

			// get shuffled player ratings
			var shuffledPlayerRatings = shuffledPlayerIndices.Select(i => players[i].Rating).ToArray();
			var shuffledApproxPlayerRatings = shuffledPlayerIndices.Select(i => players[i].ApproxSkillRating).ToArray();

			// play 1 game with each team
			for (int j = 0; j < gamesPerIteration; j++)
			{
				// pick an outcome absed on aproximated skill
				double expectedRatingsTeamA = shuffledApproxPlayerRatings.Skip(j * playersPerGame).Take(playersPerTeam).Sum();
				double expectedRatingsTeamB = shuffledApproxPlayerRatings.Skip(j * playersPerGame + playersPerTeam).Take(playersPerTeam).Sum();

				double outcome = expectedRatingsTeamA - expectedRatingsTeamB;

				outcome = ((int)Math.Clamp(outcome, -1, 1)) * 0.5 + 0.5;

				// create arrays with team ratings
				double[] ratingsTeamA = shuffledPlayerRatings.Skip(j * playersPerGame).Take(playersPerTeam).ToArray();
				double[] ratingsTeamB = shuffledPlayerRatings.Skip(j * playersPerGame + playersPerTeam).Take(playersPerTeam).ToArray();

				// calculate new ratings
				double[] expectedScoresA = EloTeamsCalculationHelper.GetExpectedScores(ratingsTeamA, ratingsTeamB);
				double[] newRatingsA = EloTeamsCalculationHelper.GetNewRatings(ratingsTeamA, expectedScoresA, outcome);

				double[] expectedScoresB = EloTeamsCalculationHelper.GetExpectedScores(ratingsTeamB, ratingsTeamA);
				double[] newRatingsB = EloTeamsCalculationHelper.GetNewRatings(ratingsTeamB, expectedScoresB, 1 - outcome);

				// put team ratings back into main arrays
				Array.Copy(newRatingsA, 0, shuffledPlayerRatings, j * playersPerGame, playersPerTeam);
				Array.Copy(newRatingsB, 0, shuffledPlayerRatings, j * playersPerGame + playersPerTeam, playersPerTeam);
			}

			// update ratings in players
			for (int j = 0; j < playerCount; j++)
			{
				players[j].Rating = shuffledPlayerRatings[unshuffledPlayerIndices[j]];
			}
		}

		double avg = players.Select(x => x.Rating).Average();

		double expectedAvg = players.Select(x => x.ApproxSkillRating).Average();

		Assert.Equal(expectedAvg, avg, 10);
	}

	private static Random r = new Random();

	private static int[] GetShuffledIndices(int count)
	{
		return Enumerable.Range(0, count).OrderBy(c => r.Next()).ToArray();
	}

	private static int[] GetUnshuffledIndices(int[] shuffled)
	{
		return Enumerable.Range(0, shuffled.Length).OrderBy(c => shuffled[c]).ToArray();
	}
}
