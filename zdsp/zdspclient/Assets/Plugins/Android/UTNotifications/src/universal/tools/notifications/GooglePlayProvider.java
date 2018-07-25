package universal.tools.notifications;

import java.io.IOException;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.AsyncTask;
import android.text.TextUtils;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.gcm.GoogleCloudMessaging;
import com.unity3d.player.UnityPlayer;

public class GooglePlayProvider {
//public
	public final String Name = "GooglePlay";
	
	public static boolean isAvailable(Context context) {
		return GooglePlayServicesUtil.isGooglePlayServicesAvailable(context) == ConnectionResult.SUCCESS;
	}
	
	// Provide Application Context here
	public GooglePlayProvider(Context context, String googlePlaySenderID) {
		m_gcm = GoogleCloudMessaging.getInstance(context);
		m_googlePlaySenderID = googlePlaySenderID;
		
		String registrationId = getRegistrationId(context);

        if (TextUtils.isEmpty(registrationId)) {
            registerInBackground(context);
        } else {
        	Manager.onRegistered(Name, registrationId);
        }
	}
	
//private
	private String getRegistrationId(Context context) {
	    final SharedPreferences prefs = getGCMPreferences(context);
	    String registrationId = prefs.getString(PROPERTY_REG_ID, "");
	    if (TextUtils.isEmpty(registrationId)) {
	        return "";
	    }
	    
	    // Check if app was updated; if so, it must clear the registration ID
	    // since the existing regID is not guaranteed to work with the new
	    // app version.
	    int registeredVersion = prefs.getInt(PROPERTY_APP_VERSION, Integer.MIN_VALUE);
	    int currentVersion = getAppVersion(context);
	    if (registeredVersion != currentVersion) {
	        return "";
	    }
	    return registrationId;
	}
	
	private SharedPreferences getGCMPreferences(Context context) {
	    // This sample app persists the registration ID in shared preferences, but
	    // how you store the regID in your app is up to you.
	    return context.getSharedPreferences(GooglePlayProvider.class.getName(), Context.MODE_PRIVATE);
	}
	
	private static int getAppVersion(Context context) {
	    try {
	        PackageInfo packageInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 0);
	        return packageInfo.versionCode;
	    } catch (NameNotFoundException e) {
	        // should never happen
	        throw new RuntimeException("Could not get package name: " + e);
	    }
	}
	
	private void storeRegistrationId(Context context, String regId) {
	    final SharedPreferences prefs = getGCMPreferences(context);
	    int appVersion = getAppVersion(context);
	    SharedPreferences.Editor editor = prefs.edit();
	    editor.putString(PROPERTY_REG_ID, regId);
	    editor.putInt(PROPERTY_APP_VERSION, appVersion);
	    editor.commit();
	}
	
	private void registerInBackground(final Context context) {
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			@Override
			public void run() {
			    new AsyncTask<Void, Void, Integer>() {
			        @Override
			        protected Integer doInBackground(Void... params) {
			        	// Limited exponential backoff is used
			        	while (true) {
			        		try {
				            	String registrationId = m_gcm.register(m_googlePlaySenderID);
			
				                // Persist the regID - no need to register again.
				                storeRegistrationId(context, registrationId);
				                Manager.onRegistered(Name, registrationId);
				                
				                break;
				            } catch (IOException ex) {
				            	try {
				            		Thread.sleep(m_backoffTime);
				            	} catch (InterruptedException e) {}
				            	
				            	if (m_backoffTime < 32000) {
				            		m_backoffTime *= 2;
				            	}
				            } catch (SecurityException ex) {
				            	Log.e("UniversalTools", "Unable to register GCM: " + ex.getMessage());
				            	break;
				            }
			        	}
			        	
			        	return 0;
			        }
		
			        @Override
			        protected void onPostExecute(Integer param) {
			        }
			        
			        private long m_backoffTime = 2000;
			    }.execute(null, null, null);
			}
		});
	}
	
	private static final String PROPERTY_REG_ID = "registration_id";
	private static final String PROPERTY_APP_VERSION = "appVersion";
	
	private GoogleCloudMessaging m_gcm;
	private String m_googlePlaySenderID;
}
