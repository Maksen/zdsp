package universal.tools.notifications;

import com.google.android.gms.gcm.GoogleCloudMessaging;

import android.app.IntentService;
import android.content.Intent;
import android.os.Bundle;

public class GcmIntentService extends IntentService {
//public
	public GcmIntentService() {
        super("GcmIntentService");
    }
	
//protected
	@Override
    protected void onHandleIntent(Intent intent) {
		try {
			final Bundle extras = intent.getExtras();
	        GoogleCloudMessaging gcm = GoogleCloudMessaging.getInstance(this);
	        String messageType = gcm.getMessageType(intent);
	
	        if (!extras.isEmpty() && GoogleCloudMessaging.MESSAGE_TYPE_MESSAGE.equals(messageType)) {
	        	Manager.postPushNotification(getApplicationContext(), extras);
	        }
		} finally {
			GcmBroadcastReceiver.completeWakefulIntent(intent);
		}
	}
}
