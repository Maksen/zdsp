package universal.tools.notifications;

import android.content.Intent;
import android.util.Log;

import com.amazon.device.messaging.ADMMessageHandlerBase;

public class AdmIntentService extends ADMMessageHandlerBase {
//public
	public AdmIntentService() {
        super("AdmIntentService");
    }
	
//protected
	@Override
    protected void onRegistered(final String newRegistrationId) {
		Manager.onRegistered(AmazonProvider.Name, newRegistrationId);
	}
	
	@Override
    protected void onUnregistered(final String registrationId) {
		Manager.onRegistered(AmazonProvider.Name, "");
	}
	
	@Override
    protected void onRegistrationError(final String errorId) {
		Log.e("UniversalTools", "Unable to register ADM ID: error " + errorId);
	}
	
	@Override
	protected void onMessage(final Intent intent) {
		Manager.postPushNotification(getApplicationContext(), intent.getExtras());
	}
}
