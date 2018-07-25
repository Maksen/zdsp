package universal.tools.notifications;

import java.net.URLDecoder;

import org.json.*;

import com.unity3d.player.UnityPlayer;

import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.SystemClock;
import android.support.v4.app.NotificationCompat;

public class Manager {
//public
	public static boolean initialize(boolean googlePlayPushNotificationsEnabled, boolean amazonPushNotificationsEnabled, String googlePlaySenderID, boolean willHandleReceivedNotifications, int startId, boolean incrementalId, int showNotificationsMode, boolean restoreScheduledOnReboot, int notificationsGroupingMode, boolean showLatestNotificationOnly) {
		m_nextPushNotificationId = startId;
		
		Context context = UnityPlayer.currentActivity.getApplicationContext();
		
		SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = prefs.edit();
	    editor.putString(MAIN_ACTIVITY_CLASS_NAME, UnityPlayer.currentActivity.getClass().getName());
	    editor.putBoolean(WILL_HANDLE_RECEIVED_NOTIFICATIONS, willHandleReceivedNotifications);
	    editor.putBoolean(INCREMENTAL_ID, incrementalId);
	    editor.putInt(START_ID, startId);
	    editor.putInt(SHOW_NOTIFICICATIONS_MODE, showNotificationsMode);
	    editor.putInt(NOTIFICATIONS_GROUPING_MODE, notificationsGroupingMode);
	    editor.putBoolean(SHOW_LATEST_NOTIFICATIONS_ONLY, showLatestNotificationOnly);
	    editor.commit();
	    
	    ScheduledNotificationsRestorer.setRestoreScheduledOnReboot(context, restoreScheduledOnReboot);
	    
	    if (googlePlayPushNotificationsEnabled) {
	    	if (GooglePlayProvider.isAvailable(context)) {
	    		m_provider = new GooglePlayProvider(context, googlePlaySenderID);
	    		return true;
			}
		}
	    if (amazonPushNotificationsEnabled) {
			if (AmazonProvider.isAvailable()) {
				m_provider = new AmazonProvider(context);
			}
			return true;
		}
	    
		return !googlePlayPushNotificationsEnabled && !amazonPushNotificationsEnabled;
	}
	
	public static void postNotification(String title, String text, int id, String[] userData, String notificationProfile) {
		postNotification(UnityPlayer.currentActivity, title, text, id, userDataToBundle(userData, title, text, notificationProfile), notificationProfile, true);
	}
	
	public static void scheduleNotification(int triggerInSeconds, String title, String text, int id, String[] userData, String notificationProfile) {
		scheduleNotificationCommon(UnityPlayer.currentActivity, false, triggerInSeconds, 0, title, text, id, userDataToBundle(userData, title, text, notificationProfile), notificationProfile);		
	}
	
	public static void scheduleNotificationRepeating(int firstTriggerInSeconds, int intervalSeconds, String title, String text, int id, String[] userData, String notificationProfile) {
		scheduleNotificationCommon(UnityPlayer.currentActivity, true, firstTriggerInSeconds, intervalSeconds, title, text, id, userDataToBundle(userData, title, text, notificationProfile), notificationProfile);
	}
	
	public static void setNotificationsEnabled(boolean enabled) {
		Context context = UnityPlayer.currentActivity.getApplicationContext();
		
		SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = prefs.edit();
	    editor.putBoolean(NOTIFICATIONS_ENABLED, enabled);
	    editor.commit();
	}
	
	public static void hideNotification(int id) {
		NotificationManager notificationManager = (NotificationManager)UnityPlayer.currentActivity.getSystemService(Context.NOTIFICATION_SERVICE);
		notificationManager.cancel(id);
	}
	
	public static void cancelNotification(int id) {
		hideNotification(id);
		
		Context context = UnityPlayer.currentActivity.getApplicationContext();
		
		PendingIntent pendingIntent = getPendingIntentForScheduledNotification(context, null, null, id, null, null, false);
		AlarmManager alarmManager = (AlarmManager)context.getSystemService(Context.ALARM_SERVICE);
		alarmManager.cancel(pendingIntent);
		
		clearSheduledNotificationId(context, id);
	}
	
	public static void hideAllNotifications() {
		hideAllNotifications(UnityPlayer.currentActivity);
	}
	
	public static void cancelAllNotifications() {
		hideAllNotifications();
		for (int id: getStoredScheduledNotificationIds(UnityPlayer.currentActivity.getApplicationContext())) {
			cancelNotification(id);
		}
	}
	
	public static boolean notificationsEnabled() {
		return notificationsEnabled(UnityPlayer.currentActivity.getApplicationContext());
	}
	
	public static String getClickedNotificationPacked() {
		Intent activityIntent = UnityPlayer.currentActivity.getIntent();
		String result = activityIntent.getStringExtra(KEY_NOTIFICATION);
		activityIntent.removeExtra(KEY_NOTIFICATION);
		
		return result;
	}
	
	public static String getReceivedNotificationsPacked() {
		Context context = UnityPlayer.currentActivity.getApplicationContext();
		String res = readReceivedNotificationsPacked(context);
		
		SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = prefs.edit();
		editor.remove(RECEIVED_NOTIFICATIONS);
	    editor.commit();
		
		return "[" + res + "]";
	}
	
	public static void setBackgroundMode(boolean backgroundMode) {
		m_backgroundMode = backgroundMode;
	}
	
//public constants
	public static final String KEY_ID = "__UT_ID";
    public static final String KEY_REPEATED = "__UT_REPEATED";
    public static final String KEY_USER_DATA = "__UT_USER_DATA";
    public static final String KEY_TITLE = "__UT_TITLE";
    public static final String KEY_TEXT = "__UT_TEXT";
    public static final String KEY_NOTIFICATION_PROFILE = "__UT_NOTIFICATION_PROFILE";
    public static final String KEY_NOTIFICATION = "universal.tools.notifications.__notification";
    public static final String MAIN_ACTIVITY_CLASS_NAME = "MAIN_ACTIVITY_CLASS_NAME";
	
//package
	static void postNotification(Context context, String title, String text, int id, Bundle userData, String notificationProfile, boolean repeatedOrNotScheduled) {
		context = context.getApplicationContext();
		
		if (context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getBoolean(SHOW_LATEST_NOTIFICATIONS_ONLY, false)) {
			hideAllNotifications(context);
		}
		
		postNotificationPrepared(context, prepareNotification(context, title, text, id, notificationProfile, userData), id, userData, repeatedOrNotScheduled);
	}
	
	static void postPushNotification(Context context, Bundle extras) {
		context = context.getApplicationContext();
		
		String title = extras.getString(TITLE);
		if (title == null) {
			title = "WRONG MESSAGE FORMAT! Push notification message must contain \"" + TITLE + "\". F.e. see Assets/UTNotifications/DemoServer/src/DemoServer/PushNotificator.java";
		}
		String text = extras.getString(TEXT);
		if (text == null) {
			text = "WRONG MESSAGE FORMAT! Push notification message must contain \"" + TEXT + "\". F.e. see Assets/UTNotifications/DemoServer/src/DemoServer/PushNotificator.java";
		}
		String notificationProfile = extras.containsKey(NOTIFICATION_PROFILE) ? extras.getString(NOTIFICATION_PROFILE) : null;
		
		extras.remove(TITLE);
		extras.remove(TEXT);
		extras.remove(NOTIFICATION_PROFILE);
		extras.putString(KEY_TITLE, title);
		extras.putString(KEY_TEXT, text);
		
    	Manager.postNotification(context, title, text, Manager.getNextPushNotificationId(context), extras, notificationProfile, true);
	}
	
	static void onRegistered(String providerName, String id) {
		if (UnityPlayer.currentActivity != null) {
			JSONArray json = new JSONArray();
			json.put(providerName);
			json.put(id);
			
			UnityPlayer.UnitySendMessage("UTNotificationsManager", "_OnAndroidIdReceived", json.toString());
		}
	}
	
	static boolean notificationsEnabled(Context context) {
		SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
		return prefs.getBoolean(NOTIFICATIONS_ENABLED, true);
	}
	
	static int getNextPushNotificationId(Context context) {
		context = context.getApplicationContext();
		
		if (m_nextPushNotificationId == -1) {
			m_nextPushNotificationId = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getInt(START_ID, 0);
		}
		
		if (context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getBoolean(INCREMENTAL_ID, false)) {
			return m_nextPushNotificationId++;
		} else {
			return m_nextPushNotificationId;
		}
	}
	
	static int[] getStoredScheduledNotificationIds(Context context) {
		String scheduledNotificationIdsString = getStoredScheduledNotificationIdsString(context);
		
		if (scheduledNotificationIdsString == null || scheduledNotificationIdsString.isEmpty()) {
			return new int[0];
		}
		
		String[] split = scheduledNotificationIdsString.split(",");
		int[] res = new int[split.length];
		for (int i = 0; i < split.length; ++i) {
			res[i] = Integer.parseInt(split[i]);
		}
		
		return res;
	}
	
	static void scheduleNotificationCommon(Context context, boolean repeated, int triggerInSeconds, int intervalSeconds, String title, String text, int id, Bundle userData, String notificationProfile) {
		context = context.getApplicationContext();
		
		PendingIntent pendingIntent = getPendingIntentForScheduledNotification(context, title, text, id, userData, notificationProfile, repeated);
		
		final long elapsedRealtime = SystemClock.elapsedRealtime();
		AlarmManager alarmManager = (AlarmManager)context.getSystemService(Context.ALARM_SERVICE);
		if (repeated) {
        	// http://developer.android.com/reference/android/app/AlarmManager.html#setInexactRepeating(int, long, long, android.app.PendingIntent)
    		// As of API 19, all repeating alarms are inexact. Because this method has been available since API 3, your application can safely call
    		// it and be assured that it will get similar behavior on both current and older versions of Android.
			alarmManager.setInexactRepeating(AlarmManager.ELAPSED_REALTIME, elapsedRealtime + (long)triggerInSeconds*1000L, (long)intervalSeconds*1000L, pendingIntent);
		} else {
			alarmManager.set(AlarmManager.ELAPSED_REALTIME, elapsedRealtime + (long)triggerInSeconds*1000L, pendingIntent);
		}
		
		storeScheduledNotificationId(context, repeated, triggerInSeconds, intervalSeconds, title, text, id, userData, notificationProfile);
	}
	
	static boolean backgroundMode() {
		return m_backgroundMode;
	}
	
//private
	private static void hideAllNotifications(Context context) {
		NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
		notificationManager.cancelAll();
	}
	
	private static void postNotificationPrepared(Context context, Notification notification, int id, Bundle userData, boolean repeatedOrNotScheduled) {
		if (notificationsEnabled(context)) {
			if (checkShowMode(context)) {
				NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
				notificationManager.notify(id, notification);
			}
			
			notificationReceived(context.getApplicationContext(), id, userData);
		}
		
		if (!repeatedOrNotScheduled) {
			clearSheduledNotificationId(context, id);
		}
	}
	
	private static PendingIntent getPendingIntentForScheduledNotification(Context context, String title, String text, int id, Bundle userData, String notificationProfile, boolean repeated) {
		Intent notificationIntent = new Intent(context, AlarmBroadcastReceiver.class);
		notificationIntent.setData(Uri.parse("custom://ut." + id));
        notificationIntent.putExtra(KEY_ID, id);
        if (repeated) {
        	notificationIntent.putExtra(KEY_REPEATED, true);
        }
        if (userData != null) {
        	notificationIntent.putExtra(KEY_USER_DATA, userData);
        }
        
        return PendingIntent.getBroadcast(context, id, notificationIntent, PendingIntent.FLAG_CANCEL_CURRENT);
	}
	
	@SuppressWarnings("deprecation")
	private static Notification prepareNotification(Context context, String title, String text, int id, String notificationProfile, Bundle userData) {
		Resources res = context.getResources();
		
		//Find an icon
	    int icon = 0;
	    if (notificationProfile != null && !notificationProfile.isEmpty()) {
	    	//Check if need to use Android 5.0+ version of the icon
	    	if (android.os.Build.VERSION.SDK_INT >= 21) {
	    		icon = res.getIdentifier(notificationProfile + "_android5plus", "drawable", context.getPackageName());
	    	}
	    	
	    	if (icon == 0) {
	    		icon = res.getIdentifier(notificationProfile, "drawable", context.getPackageName());
	    	}
	    }
	    if (icon == 0) {
	    	icon = res.getIdentifier("app_icon", "drawable", context.getPackageName());
	    }
	    
	    //Find a custom large icon if specified
	    int largeIcon = 0;
	    if (notificationProfile != null && !notificationProfile.isEmpty()) {
	    	largeIcon = res.getIdentifier(notificationProfile + "_large", "drawable", context.getPackageName());
	    }
	    
	    //Find a custom sound if specified
	    int soundId = 0;
	    if (notificationProfile != null && !notificationProfile.isEmpty()) {
	    	soundId = res.getIdentifier(notificationProfile, "raw", context.getPackageName());
	    }
	    
	    title = URLDecoder.decode(title);
	    text = URLDecoder.decode(text);
	    
	    Intent notificationIntent = new Intent(context, NotificationIntentService.class);
	    notificationIntent.putExtra(KEY_NOTIFICATION, packReceivedNotification(id, userData));
	    PendingIntent contentIntent = PendingIntent.getService(context, id, notificationIntent, PendingIntent.FLAG_UPDATE_CURRENT);
	    
	    NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
											.setSmallIcon(icon)
											.setContentTitle(title)
											.setDefaults((soundId == 0) ? NotificationCompat.DEFAULT_ALL : NotificationCompat.DEFAULT_LIGHTS | NotificationCompat.DEFAULT_VIBRATE)
											.setContentText(text)
											.setContentIntent(contentIntent)
											.setAutoCancel(true);
	    
	    if (largeIcon != 0) {
	    	builder.setLargeIcon(BitmapFactory.decodeResource(res, largeIcon));
	    }
	    
	    if (soundId != 0) {
	    	builder.setSound(Uri.parse("android.resource://" + context.getPackageName() + "/raw/" + notificationProfile));
	    }
	    
	    boolean hasGroup = false;
	    int groupingMode = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getInt(NOTIFICATIONS_GROUPING_MODE, 0);
	    switch (groupingMode) {
	    //NONE
	    case 0:
	    	break;
	    
	    //BY_NOTIFICATION_PROFILES
	    case 1:
	    	if (notificationProfile != null && !notificationProfile.isEmpty()) {
	    		builder.setGroup(notificationProfile);
	    		hasGroup = true;
	    	}
	    	break;
	    	
	    //FROM_USER_DATA
	    case 2:
	    	if (userData.containsKey("notification_group")) {
	    		builder.setGroup(userData.getString("notification_group"));
	    		hasGroup = true;
	    	}
	    	break;
	    	
    	//ALL_IN_A_SINGLE_GROUP
	    case 3:
	    	builder.setGroup("__ALL");
	    	hasGroup = true;
	    	break;
	    }
	    
	    if (hasGroup && userData.containsKey("notification_group_summary")) {
	    	builder.setGroupSummary(true);
	    }
		
		return builder.build();
	}
	
	private static Bundle userDataToBundle(String[] userData, String title, String text, String notificationProfile) {
		Bundle bundle = new Bundle();
		
		bundle.putString(KEY_TITLE, title);
		bundle.putString(KEY_TEXT, text);
		bundle.putString(KEY_NOTIFICATION_PROFILE, notificationProfile);
		
		if (userData != null) {
			for (int i = 1; i < userData.length; i += 2) {
				bundle.putString(userData[i - 1], userData[i]);
			}
		}
		
		return bundle;
	}
	
	private static String packReceivedNotification(int id, Bundle userData) {
		try {
			JSONObject json = new JSONObject();
			
			json.put("title", userData.getString(KEY_TITLE));
			json.put("text", userData.getString(KEY_TEXT));
			json.put("id", id);
			json.put("notification_profile", userData.getString(KEY_NOTIFICATION_PROFILE));
			
			JSONObject userDataJson = new JSONObject();
			
			for (String key: userData.keySet()) {
				if (!KEY_TITLE.equals(key) && !KEY_TEXT.equals(key) && !KEY_NOTIFICATION_PROFILE.equals(key)) {
					String val = userData.getString(key);
					if (val != null) {
						try {
							userDataJson.put(key, val);
						} catch (JSONException e) {
							e.printStackTrace();
						}
					}
				}
			}
			
			json.put("user_data", userDataJson);
			
			return json.toString();
		} catch (JSONException e) {
			e.printStackTrace();
			return null;
		}
	}
	
	private static void notificationReceived(Context context, int id, Bundle userData) {
		boolean willHandleReceivedNotifications = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getBoolean(WILL_HANDLE_RECEIVED_NOTIFICATIONS, false);
		
		if (willHandleReceivedNotifications)
		{
			String receivedNotificationsPacked = readReceivedNotificationsPacked(context);
			String receivedPacked = packReceivedNotification(id, userData);
			
			if (receivedPacked != null) {
				if (receivedNotificationsPacked != null && !receivedNotificationsPacked.isEmpty()) {
					receivedNotificationsPacked += ',' + receivedPacked;
				} else {
					receivedNotificationsPacked = receivedPacked;
				}
				
				SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
				SharedPreferences.Editor editor = prefs.edit();
				editor.putString(RECEIVED_NOTIFICATIONS, receivedNotificationsPacked);
			    editor.commit();
			}
		}
	}
	
	private static String readReceivedNotificationsPacked(Context context) {
		return context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getString(RECEIVED_NOTIFICATIONS, "");
	}
	
	private static String getStoredScheduledNotificationIdsString(Context context) {
		return context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getString(SCHEDULED_NOTIFICATION_IDS, "");
	}
	
	private static void storeScheduledNotificationId(Context context, boolean repeated, int triggerInSeconds, int intervalSeconds, String title, String text, int id, Bundle userData, String notificationProfile) {
		ScheduledNotificationsRestorer.register(context, repeated, triggerInSeconds, intervalSeconds, title, text, id, userData, notificationProfile);
		
		int[] ids = getStoredScheduledNotificationIds(context);
		for (int storedId: ids) {
			if (storedId == id) {
				return;
			}
		}
		
		String scheduledNotificationIdsString = getStoredScheduledNotificationIdsString(context);
		if (scheduledNotificationIdsString == null || scheduledNotificationIdsString.isEmpty()) {
			scheduledNotificationIdsString = "" + id;
		} else {
			scheduledNotificationIdsString += "," + id;
		}
		
		SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = prefs.edit();
		editor.putString(SCHEDULED_NOTIFICATION_IDS, scheduledNotificationIdsString);
	    editor.commit();
	}
	
	private static void clearSheduledNotificationId(Context context, int id) {
		String idAsString = Integer.toString(id); 
		
		String scheduledNotificationIdsString = getStoredScheduledNotificationIdsString(context);
		if (scheduledNotificationIdsString != null && !scheduledNotificationIdsString.isEmpty()) {
			boolean found = true;
			if (scheduledNotificationIdsString == idAsString) {
				scheduledNotificationIdsString = "";
				found = true;
			} else {
				if (scheduledNotificationIdsString.indexOf("," + idAsString + ",") >= 0) {
					scheduledNotificationIdsString = scheduledNotificationIdsString.replace("," + idAsString + ",", ",");
					found = true;
				} else if (scheduledNotificationIdsString.startsWith(idAsString + ",")) {
					scheduledNotificationIdsString = scheduledNotificationIdsString.substring(idAsString.length() + 1);
					found = true;
				} else if (scheduledNotificationIdsString.endsWith("," + idAsString)) {
					scheduledNotificationIdsString = scheduledNotificationIdsString.substring(0, scheduledNotificationIdsString.length() - (idAsString.length() + 1));
					found = true;
				}
			}
			
			if (found) {
				SharedPreferences prefs = context.getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE);
				SharedPreferences.Editor editor = prefs.edit();
				editor.putString(SCHEDULED_NOTIFICATION_IDS, scheduledNotificationIdsString);
			    editor.commit();
			}
		}
		
		ScheduledNotificationsRestorer.cancel(context, id);
	}
	
	private static boolean checkShowMode(Context context) {
		int showNotificationsMode = context.getApplicationContext().getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getInt(SHOW_NOTIFICICATIONS_MODE, 0);
		
		switch (showNotificationsMode) {
		//WHEN_CLOSED_OR_IN_BACKGROUND
		case 0:
			return UnityPlayer.currentActivity == null || backgroundMode();
			
		//WHEN_CLOSED
		case 1:
			return UnityPlayer.currentActivity == null;
		
		//ALWAYS
		default:
			return true;
		}
	}
	
	@SuppressWarnings("unused")
	private static Object m_provider;
	private static final String WILL_HANDLE_RECEIVED_NOTIFICATIONS = "WILL_HANDLE_RECEIVED_NOTIFICATIONS";
	private static final String INCREMENTAL_ID = "INCREMENTAL_ID";
	private static final String START_ID = "START_ID";
	private static final String SHOW_NOTIFICICATIONS_MODE = "SHOW_NOTIFICICATIONS_MODE";
	private static final String NOTIFICATIONS_GROUPING_MODE = "NOTIFICATIONS_GROUPING_MODE";
	private static final String SHOW_LATEST_NOTIFICATIONS_ONLY = "SHOW_LATEST_NOTIFICATIONS_ONLY";
	private static final String NOTIFICATIONS_ENABLED = "NOTIFICATIONS_ENABLED";
	private static final String RECEIVED_NOTIFICATIONS = "RECEIVED_NOTIFICATIONS";
	private static final String SCHEDULED_NOTIFICATION_IDS = "SCHEDULED_NOTIFICATION_IDS";
	private static final String TITLE = "title";
	private static final String TEXT = "text";
	private static final String NOTIFICATION_PROFILE = "notification_profile";
	private static int m_nextPushNotificationId = -1;
	private static boolean m_backgroundMode = false;
}
