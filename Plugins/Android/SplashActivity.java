package com.pluginset.quicksdk;

import android.content.Intent;
import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;

import com.pluginset.devices.Utils;
import com.quicksdk.QuickSdkSplashActivity;

public class SplashActivity extends QuickSdkSplashActivity {
    private static final String MainActivityName = "com.pluginset.quicksdk.PluginSetQuickActivity";
    private static final String QuickSplashBackgroundKey = "quick_splash_background";

    @Override
    public int getBackgroundColor() {
        return Utils.GetApplicationMetaInt(this, QuickSplashBackgroundKey, Color.BLACK);
    }
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
    	// TODO Auto-generated method stub
    	changeScreen();
    	super.onCreate(savedInstanceState);
    }

    @Override
    public void onSplashStop() {
        try {
            Class mainClass = Class.forName(MainActivityName);
            Intent intent = new Intent(this, mainClass);
            startActivity(intent);
            finish();
        } catch (ClassNotFoundException e) {
            e.printStackTrace();
        }
    }
    
    private void changeScreen(){
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.P){
            Log.d("SplashActivity", "changeScreen2222: ");
            WindowManager.LayoutParams lp = getWindow().getAttributes();
            lp.layoutInDisplayCutoutMode = WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES;
            getWindow().setAttributes(lp);

            View decorView = getWindow().getDecorView();
            decorView.setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN | View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        }
    }
}
