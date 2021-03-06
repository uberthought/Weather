package com.companyname.weather.fragments

import android.os.Bundle
import android.os.Handler
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.lifecycle.ViewModelProvider
import androidx.navigation.fragment.findNavController
import com.companyname.weather.R
import com.companyname.weather.databinding.ForecastFragmentBinding
import com.companyname.weather.services.LocationService
import com.companyname.weather.viewModels.ConditionsViewModel
import com.companyname.weather.viewModels.LocationViewModel
import kotlinx.android.synthetic.main.activity_main.*
import kotlinx.android.synthetic.main.nav_header_main.view.*

class ForecastFragment: Fragment() {

    private lateinit var locationViewModel: LocationViewModel
    lateinit var conditionsViewModel: ConditionsViewModel

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        locationViewModel = ViewModelProvider(this)[LocationViewModel::class.java]
        conditionsViewModel = ViewModelProvider(this)[ConditionsViewModel::class.java]

        val binding = ForecastFragmentBinding.inflate(layoutInflater)
        binding.lifecycleOwner = this
        binding.conditionsViewModel = conditionsViewModel
        locationViewModel.location.observe(viewLifecycleOwner, androidx.lifecycle.Observer {location ->
            (activity as AppCompatActivity).supportActionBar?.title = location
        })
        conditionsViewModel.timestamp.observe(viewLifecycleOwner, androidx.lifecycle.Observer { timestamp ->
            (activity as AppCompatActivity).nav_view.textView?.text = timestamp
            binding.timestamp.invalidate()
        })

        Handler().postDelayed({
            if (LocationService.hasPermission.value == true)
                return@postDelayed
            LocationService.location.value?.let {
                if (it.latitude == 0.0 && it.longitude == 0.0)
                    findNavController().navigate(R.id.action_nav_forecast_to_nav_map)
            }
        }, 200)

        return binding.root
    }
}