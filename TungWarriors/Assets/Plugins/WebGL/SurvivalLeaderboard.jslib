mergeInto(LibraryManager.library, {
  SubmitSurvivalLeaderboardScore_js: function(score, requestIdPtr) {
	try {
	  var requestId = UTF8ToString(requestIdPtr);
	  console.log('[SurvivalLeaderboard] SubmitSurvivalLeaderboardScore_js', score, requestId);
	  // No-op fallback for builds without real leaderboard backend.
	} catch (e) {
	  console.error('[SurvivalLeaderboard] Submit error', e);
	}
  },

  LoadSurvivalLeaderboardTop_js: function(requestIdPtr, quantityTop, includeCurrentPlayer) {
	try {
	  var requestId = UTF8ToString(requestIdPtr);
	  console.log('[SurvivalLeaderboard] LoadSurvivalLeaderboardTop_js', requestId, quantityTop, includeCurrentPlayer);
	  // No-op fallback.
	} catch (e) {
	  console.error('[SurvivalLeaderboard] Load top error', e);
	}
  },

  LoadSurvivalLeaderboardPlayerEntry_js: function(requestIdPtr) {
	try {
	  var requestId = UTF8ToString(requestIdPtr);
	  console.log('[SurvivalLeaderboard] LoadSurvivalLeaderboardPlayerEntry_js', requestId);
	  // No-op fallback.
	} catch (e) {
	  console.error('[SurvivalLeaderboard] Load player entry error', e);
	}
  },

  // Some generated glue may call SurvivalLeaderboardEnsureMethod; provide harmless fallback
  SurvivalLeaderboardEnsureMethod: function() {
	return 0;
  }
});
