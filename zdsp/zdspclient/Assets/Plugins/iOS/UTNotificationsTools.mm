#import <Foundation/Foundation.h>
#include "UTNotificationsTools.h"

#define NO_NOTIFICATION_ID ((int)(-10000001))

static int localNotificationWasClicked = NO_NOTIFICATION_ID;
static NSString* pushNotificationWasClicked = nil;

extern "C"
{
	bool _UT_RegisterForIOS8(bool remote)
	{
		float version = [[[UIDevice currentDevice] systemVersion] floatValue];
		if (version >= 8.0)
		{
			UIUserNotificationType types = UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeAlert | UIRemoteNotificationTypeSound;
			UIUserNotificationSettings* settings = [UIUserNotificationSettings settingsForTypes:types categories:nil];
			[[UIApplication sharedApplication] registerUserNotificationSettings:settings];

			if (remote)
			{
				[[UIApplication sharedApplication] registerForRemoteNotifications];
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	int _UT_GetIconBadgeNumber()
	{
		return (int)[UIApplication sharedApplication].applicationIconBadgeNumber;
	}

	void _UT_SetIconBadgeNumber(int value)
	{
		[UIApplication sharedApplication].applicationIconBadgeNumber = value;
	}

	void _UT_HideAllPushNotifications()
	{
		int oldBadgeNumber = _UT_GetIconBadgeNumber();
		_UT_SetIconBadgeNumber(0);
		_UT_SetIconBadgeNumber(oldBadgeNumber);
    }
    
    bool _UT_LocalNotificationWasClicked(int id)
    {
        if (localNotificationWasClicked == id)
        {
            _UT_SetLocalNotificationWasClicked(nil);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    bool _UT_PushNotificationWasClicked(const char* body)
    {
        if (pushNotificationWasClicked && body)
        {
            bool clicked = !strcmp(body, [pushNotificationWasClicked UTF8String]);
            if (clicked)
            {
                _UT_SetPushNotificationWasClicked(nil);
                return true;
            }
        }

        return false;
    }
}

void _UT_SetLocalNotificationWasClicked(NSDictionary* userInfo)
{
    if (!userInfo)
    {
        localNotificationWasClicked = NO_NOTIFICATION_ID;
    }
    else
    {
        localNotificationWasClicked = [[userInfo objectForKey:@"_UT_ID"] intValue];
    }
}

void _UT_SetPushNotificationWasClicked(NSDictionary* userInfo)
{
    NSObject* alert = [[userInfo objectForKey:@"aps"] objectForKey:@"alert"];
    if (alert)
    {
        NSString* body;
        if ([alert isKindOfClass:[NSString class]])
        {
            body = (NSString*)alert;
        }
        else
        {
            body = [(NSDictionary*)alert objectForKey:@"body"];
        }
        
        pushNotificationWasClicked = body;
    }
}