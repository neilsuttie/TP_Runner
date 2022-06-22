using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Prefill the info on the player data, as they will be used to populate the leadboard.
public class Leaderboard : MonoBehaviour
{
	public RectTransform entriesRoot;
	public int entriesCount;

	public HighscoreUI playerEntry;
	public bool forcePlayerDisplay;
	public bool displayPlayer = true;

	//Used to toggle leaderboards
	public Button GlobalButton;
	//Used to manage state of the leaderboard
	public bool IsGlobalLeaderboard = false;
	//Max global leader score
	[Range(1, 10)]
	public int maxGlobalScores = 3;

	public void Open()
	{
		gameObject.SetActive(true);

		Populate();
	}

	public void OpenGlobal()
    {
		RequestOnlineLeaderboard();
    }

	public void Close()
	{
		gameObject.SetActive(false);
	}

	public void Populate()
	{
		// Start by making all entries enabled & putting player entry last again.
		playerEntry.transform.SetAsLastSibling();
		for(int i = 0; i < entriesCount; ++i)
		{
			entriesRoot.GetChild(i).gameObject.SetActive(true);
		}

		// Find all index in local page space.
		int localStart = 0;
		int place = -1;
		int localPlace = -1;

		if (displayPlayer)
		{
			place = PlayerData.instance.GetScorePlace(int.Parse(playerEntry.score.text));
			localPlace = place - localStart;
		}

		if (localPlace >= 0 && localPlace < entriesCount && displayPlayer)
		{
			playerEntry.gameObject.SetActive(true);
			playerEntry.transform.SetSiblingIndex(localPlace);
		}

		if (!forcePlayerDisplay || PlayerData.instance.highscores.Count < entriesCount)
			entriesRoot.GetChild(entriesRoot.transform.childCount - 1).gameObject.SetActive(false);

		int currentHighScore = localStart;

		for (int i = 0; i < entriesCount; ++i)
		{
			HighscoreUI hs = entriesRoot.GetChild(i).GetComponent<HighscoreUI>();

            if (hs == playerEntry || hs == null)
			{
				// We skip the player entry.
				continue;
			}

		    if (PlayerData.instance.highscores.Count > currentHighScore)
		    {
		        hs.gameObject.SetActive(true);
		        hs.playerName.text = PlayerData.instance.highscores[currentHighScore].name;
		        hs.number.text = (localStart + i + 1).ToString();
		        hs.score.text = PlayerData.instance.highscores[currentHighScore].score.ToString();

		        currentHighScore++;
		    }
		    else
		        hs.gameObject.SetActive(false);
		}

		// If we force the player to be displayed, we enable it even if it was disabled from elsewhere
		if (forcePlayerDisplay) 
			playerEntry.gameObject.SetActive(true);

		playerEntry.number.text = (place + 1).ToString();
	}

	public void ToggleOpenGlobal()
	{
		if (IsGlobalLeaderboard)
		{
			IsGlobalLeaderboard = false;
			Populate();
		}
		else
		{
			//This is the PlayFab API used to get the leaderboard called "HighScore"
			RequestOnlineLeaderboard();
		}
	}

	public void RequestOnlineLeaderboard()
	{
		PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
		{
			StatisticName = "HighScore",
			StartPosition = 0,
			MaxResultsCount = maxGlobalScores
		}, result => DisplayLeaderboard(result), FailureCallback);
	}

    private void DisplayLeaderboard(GetLeaderboardResult result)
    {
		IsGlobalLeaderboard = true;
		PopulateGlobal(result.Leaderboard);
	}

	private void FailureCallback(PlayFabError error)
	{
		Debug.LogWarning("Something went wrong with your API call. Here's some debug information:");
		Debug.LogError(error.GenerateErrorReport());
	}

	private void PopulateGlobal(List<PlayerLeaderboardEntry> resultLeaderboard)
	{
		playerEntry.transform.SetAsLastSibling();

		//clear all entries and set data.
		for (int i = 0; i < entriesCount; ++i)
		{
			HighscoreUI hs = entriesRoot.GetChild(i).GetComponent<HighscoreUI>();
			entriesRoot.GetChild(i).gameObject.SetActive(true);

			if (i >= resultLeaderboard.Count)
			{
				hs.playerName.text = string.Empty;
				hs.number.text = string.Empty;
				hs.score.text = string.Empty;
			}
			else
			{
				var lbEntry = resultLeaderboard[i];
				hs.playerName.text = !string.IsNullOrEmpty(lbEntry.DisplayName) ? lbEntry.DisplayName : lbEntry.PlayFabId;
				hs.number.text = lbEntry.Position.ToString("00");
				hs.score.text = lbEntry.StatValue.ToString("000");
			}
		}
	}
}
