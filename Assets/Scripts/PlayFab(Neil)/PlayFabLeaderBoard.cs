using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabLeaderBoard : MonoBehaviour
{
	static public PlayFabLeaderBoard instance { get { return s_Instance; } }
	static protected PlayFabLeaderBoard s_Instance;

	public static GetLeaderboardResult leaderBoard;
	public static int maxScoresToFetch = 3;

	public static void SubmitScore(int playerScore)
	{
		PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
		{
			Statistics = new List<StatisticUpdate> {
			new StatisticUpdate {
				StatisticName = "HighScore",
				Value = playerScore
			}
		}
		}, result => OnStatisticsUpdated(result), FailureCallback);
	}

	private static void OnStatisticsUpdated(UpdatePlayerStatisticsResult updateResult)
	{
		Debug.Log("Successfully submitted high score");
	}

	//Get the players with the top N high scores in the game
	public static void RequestLeaderboard()
	{
		PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
		{
			StatisticName = "HighScore",
			StartPosition = 0,
			MaxResultsCount = 3
		}, result => DisplayLeaderboard(result), FailureCallback);
	}

	private static void DisplayLeaderboard(GetLeaderboardResult result)
	{
		leaderBoard = result;
	}

	private static void FailureCallback(PlayFabError error)
	{
		Debug.LogWarning("Something went wrong with your API call. Here's some debug information:");
		Debug.LogError(error.GenerateErrorReport());
	}

}
