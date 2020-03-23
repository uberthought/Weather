package com.companyname.weather

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.Toolbar
import androidx.drawerlayout.widget.DrawerLayout
import androidx.navigation.findNavController
import androidx.navigation.ui.AppBarConfiguration
import androidx.navigation.ui.navigateUp
import androidx.navigation.ui.setupActionBarWithNavController
import androidx.navigation.ui.setupWithNavController
import com.companyname.weather.fragments.PreferencesFragment
import com.companyname.weather.services.LocationService
import com.companyname.weather.services.RefreshService
import com.google.android.gms.location.*
import com.google.android.material.navigation.NavigationView

class MainActivity : AppCompatActivity() {
    companion object {
        const val LOCATION_REQUEST: Int = 1
    }

    private lateinit var fusedLocationClient:FusedLocationProviderClient
    private lateinit var appBarConfiguration: AppBarConfiguration

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContentView(R.layout.activity_main)

        val toolbar: Toolbar = findViewById(R.id.toolbar)
        setSupportActionBar(toolbar)

        val drawerLayout: DrawerLayout = findViewById(R.id.drawer_layout)
        val navView: NavigationView = findViewById(R.id.nav_view)
        val navController = findNavController(R.id.nav_host_fragment)
        // Passing each menu ID as a set of Ids because each
        // menu should be considered as top level destinations.
        appBarConfiguration = AppBarConfiguration(setOf(
            R.id.nav_forecast, R.id.nav_map, R.id.nav_settings), drawerLayout)
        setupActionBarWithNavController(navController, appBarConfiguration)
        navView.setupWithNavController(navController)

        // needs to happen before requesting location
        PreferencesFragment.update(applicationContext)
        PreferencesFragment.useDevice.observe(this, androidx.lifecycle.Observer { useDevice ->
            if (useDevice)
                LocationService().requestPermissions(this)
            else
                PreferencesFragment.lastLocation.value?.let { LocationService().setLocation(it) }
        })

        LocationService().requestPermissions(this)
    }

    override fun onResume() {
        super.onResume()

        LocationService().resume(applicationContext)
        RefreshService().resume(applicationContext)
    }

    override fun onPause() {
        super.onPause()
        RefreshService().pause()
        LocationService().pause()
    }

    override fun onSupportNavigateUp(): Boolean {
        val navController = findNavController(R.id.nav_host_fragment)
        return navController.navigateUp(appBarConfiguration) || super.onSupportNavigateUp()
    }

    override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<out String>, grantResults: IntArray) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        when (requestCode) {
            LOCATION_REQUEST -> {
                PreferencesFragment.setUseDevice(applicationContext, true)
                LocationService().resume(applicationContext)
            }
            else -> PreferencesFragment.setUseDevice(applicationContext, false)
        }
    }

}
