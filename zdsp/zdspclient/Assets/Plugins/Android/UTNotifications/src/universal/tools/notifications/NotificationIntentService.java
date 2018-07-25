package universal.tools.notifications;

import com.unity3d.player.UnityPlayer;

import android.app.IntentService;
import android.content.Context;
import android.content.Intent;

public class NotificationIntentService extends IntentService {
//public
	public NotificationIntentService() {
        super("NotificationIntentService");
    }
	
//protected
	@Override
    protected void onHandleIntent(Intent intent) {
		Class<?> mainActivityClass;
	    if (UnityPlayer.currentActivity != null) {
	    	mainActivityClass = UnityPlayer.currentActivity.getClass();
	    } else {
	    	try {
	    		mainActivityClass = Class.forName(getApplicationContext().getSharedPreferences(Manager.class.getName(), Context.MODE_PRIVATE).getString(Manager.MAIN_ACTIVITY_CLASS_NAME, "com.unity3d.player.UnityPlayerNativeActivity"));
	    	} catch (ClassNotFoundException e) {
	    		e.printStackTrace();
	    		return;
	    	}
	    }
		
		Intent mainActivityIntent = new Intent(this, mainActivityClass);
		mainActivityIntent.setAction(Intent.ACTION_MAIN);
		mainActivityIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
		mainActivityIntent.addCategory(Intent.CATEGORY_LAUNCHER);
		mainActivityIntent.putExtras(intent);
		startActivity(mainActivityIntent);
		
		if (UnityPlayer.currentActivity != null) {
			UnityPlayer.currentActivity.getIntent().putExtras(intent);
		}
	}
}
