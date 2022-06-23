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

	[Header("Number of Entries to Display")]
	[Range(1,10)]
	public int entriesCount;

	[Header("Do we display player entries?")]
	public HighscoreUI playerEntry;
	public bool forcePlayerDisplay;
	public bool displayPlayer = true;


	[Header("Global LeaderBoard Fields")]
	public Button GlobalButton;
	public string StatisticsName = "HighScore";
	//Used to manage state of the leaderboard
	public bool IsGlobalLeaderboard = false;

    #region EntryAndExitPoints
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
    #endregion

    #region PopulateLists

    /// <summary>
    /// Populate the list with locally saved entries.
    /// </summary>
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


	private void PopulateGlobal(List<PlayerLeaderboardEntry> resultLeaderboard)
	{
		//Just use the global ranking
		playerEntry.transform.SetAsLastSibling();
		playerEntry.gameObject.SetActive(false);

		//clear all entries and set data.
		for (int i = 0; i <= entriesCount; i++)
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
				int pos = lbEntry.Position + 1;
				hs.number.text = pos.ToString("00");
				hs.score.text = lbEntry.StatValue.ToString("000");
			}
		}
	}
    #endregion

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

    #region PlayFabLeaderboardFunction
    public void RequestOnlineLeaderboard()
	{
		PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
		{
			StatisticName = StatisticsName,
			StartPosition = 0,
			MaxResultsCount = entriesCount
		}, result => DisplayLeaderboard(result), FailureCallback);
	}

    private void DisplayLeaderboard(GetLeaderboardResult result)
    {
		//We've successfully updated the online score. Populate the list.
		IsGlobalLeaderboard = true;
		PopulateGlobal(result.Leaderboard);
	}

	private void FailureCallback(PlayFabError error)
	{
		Debug.LogWarning("Something went wrong with your API call. Here's some debug information:");
		Debug.LogError(error.GenerateErrorReport());
	}
    #endregion
}
