# Welcome to the Ballerina React-Native Expo app 👋

This is an [Expo](https://expo.dev) project created with [`create-expo-app`](https://www.npmjs.com/package/create-expo-app).

## Get started

1. Install dependencies

   ```bash
   yarn
   ```

2. Start the app

   ```bash
    npx expo start
   ```

In the output, you'll find options to open the app in a

- [development build](https://docs.expo.dev/develop/development-builds/introduction/)
- [Android emulator](https://docs.expo.dev/workflow/android-studio-emulator/)
- [iOS simulator](https://docs.expo.dev/workflow/ios-simulator/)
- [Expo Go](https://expo.dev/go), a limited sandbox for trying out app development with Expo

You can start developing by editing the files inside the **app** directory.

Running with Expo Go (recommended for the demo), requires no additional set up other than the simulators.

If you want to run a local build, we recommend using Expo EAS to run a local build. To do this, you will need to do some set up.

- Install Xcode (mac only) and/or Android Sutdio
- Install the latest iOS version in Xcode
  - Xcode -> Settings -> Platforms -> iOS X.X (Get)
- Make sure you have a simulator created that uses this
  - Window -> Devices and Simulators -> Create a simulator using the latest version
- Please make sure that your homebrew is set up properly by running `brew upgrade && brew doctor` and fixing any errors
- perform any commands brew doctor instructs, like adding config to your ~/.zshrc file
- (ios) install cocoapods with `gem install cocoapods` **do not use brew to install cocoapods!**
- install expo-cli `npm i -g expo-cli`
- install fastlane
  - `brew install fastlane`
  - TODO: fastlane itself recommends a different installer which may be faster, but this works for now
- to build a local build
  - `eas build -p ios --profile preview --local`
- run it on a simulator
  - recommend: download expo orbit

- Common isuses can be mostly resolved by following this guide:
- https://www.moncefbelyamani.com/how-to-install-xcode-homebrew-git-rvm-ruby-on-mac/
- be sure to follow this guide closely
- if you don't follow this guide, and try and solve issues with stackoverflow/github searches, you will likely not solve the root problem and although it looks like you are making progress, you will continue to have errors
- Some reasons you might have errors are:
  - You migrated your mac from intel to silicon (M1+), so homebrew is installed in the wrong location
  - you've installed dependencies like Cocoapods (ios package manager) using brew instead of Gem (the ruby package manager)
  - You have various versions of ruby installed with different package managers
  - You have incorrect/obsolete path references in your $PATH

**important**
If you are fixing various things, particularly removing references from your path and .zshrc or .bash_profile file, you should restart your terminal. Reloading the source doesn't refresh the path for the terminal and this can cause problems.

- Once brew doctor gives you the `Your system is ready to brew` message, you can move on to the next step, installing
- `brew install chruby ruby-install`
- `ruby-install ruby`
