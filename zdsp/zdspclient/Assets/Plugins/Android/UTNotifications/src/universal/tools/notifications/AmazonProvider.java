package universal.tools.notifications;

import android.content.Context;
import android.util.Log;

import com.amazon.device.messaging.ADM;

public class AmazonProvider {
	public static final String Name = "Amazon";
	
	public static boolean isAvailable() {
		try {
		    Class.forName("com.amazon.device.messaging.ADM");
		    return true;
		} catch (ClassNotFoundException e) {
		    return false;
		}
	}
	
	public AmazonProvider(Context context) {
		try {
			final ADM adm = new ADM(context);
			
			String registrationId = adm.getRegistrationId();
			if (registrationId == null) {
			   adm.startRegister();
			} else {
				Manager.onRegistered(Name, registrationId);
			}
		} catch (SecurityException ex) {
			Log.e("UniversalTools", "Unable to register ADM: " + ex.getMessage());
		}
	}
}
