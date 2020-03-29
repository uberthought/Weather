package com.companyname.weather.fragments

import android.os.Bundle
import android.view.*
import androidx.fragment.app.Fragment
import androidx.lifecycle.Observer
import androidx.navigation.fragment.findNavController
import com.companyname.weather.R
import com.companyname.weather.databinding.SettingsFragmentBinding
import com.companyname.weather.services.PreferenceService


class SettingsFragment : Fragment() {

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        val binding = SettingsFragmentBinding.inflate(layoutInflater)

        PreferenceService.useDevice.observe(viewLifecycleOwner, Observer {
            binding.useDeviceLocation.isChecked = it
            if (it) {
                binding.deviceLocationLabel.setTextColor(resources.getColor(R.color.disabledTextColor, null))
                binding.deviceLocationText.setTextColor(resources.getColor(R.color.disabledTextColor, null))
            }
            else {
                binding.deviceLocationLabel.setTextColor(resources.getColor(R.color.primaryTextColor, null))
                binding.deviceLocationText.setTextColor(resources.getColor(R.color.secondaryTextColor, null))
            }
        })
        PreferenceService.locationName.observe(viewLifecycleOwner, Observer { binding.deviceLocationText.text = it })

        binding.useDeviceLocationLabel.setOnClickListener { binding.useDeviceLocation.isChecked = !binding.useDeviceLocation.isChecked }
        binding.useDeviceLocation.setOnCheckedChangeListener { _, isChecked ->
            if (PreferenceService.useDevice.value != isChecked)
                PreferenceService.useDevice.value = isChecked
        }

        binding.deviceLocationLabel.setOnClickListener { deviceLocationClicked() }
        binding.deviceLocationText.setOnClickListener { deviceLocationClicked() }

        return binding.root
    }

    private fun deviceLocationClicked() {
        if (PreferenceService.useDevice.value == false)
            findNavController().navigate(R.id.action_nav_settings_to_nav_map)
    }
}
