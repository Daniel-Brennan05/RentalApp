FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install .NET SDK 10
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh

# Install Java JDK (required for Android SDK) and unzip
# Create a fixed symlink so JAVA_HOME works on both amd64 and arm64
RUN apt-get update && apt-get install -y openjdk-17-jdk unzip \
    && ln -sfn "$(dirname $(dirname $(readlink -f $(which javac))))" /usr/lib/jvm/java-17

# Pre-install NuGet workload packs that fail over Docker Desktop's virtualised network.
# The script downloads each pack in 8 MB Range chunks, bypassing the connection drops
# that occur with single large transfers, then installs them so dotnet skips the downloads.
COPY pre_install_packs.py /tmp/pre_install_packs.py
RUN python3 -u /tmp/pre_install_packs.py && rm /tmp/pre_install_packs.py

# Install MAUI workloads (maui-android only — iOS/macOS targets cannot install on Linux)
RUN dotnet workload install maui-android

# Install Android SDK command-line tools
ENV ANDROID_HOME=/opt/android-sdk
ENV JAVA_HOME=/usr/lib/jvm/java-17
RUN mkdir -p ${ANDROID_HOME}/cmdline-tools \
    && wget https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip \
    && unzip commandlinetools-linux-*.zip -d ${ANDROID_HOME}/cmdline-tools \
    && mv ${ANDROID_HOME}/cmdline-tools/cmdline-tools ${ANDROID_HOME}/cmdline-tools/latest \
    && rm commandlinetools-linux-*.zip

# Accept licenses and install platform tools
RUN yes | ${ANDROID_HOME}/cmdline-tools/latest/bin/sdkmanager --licenses && \
    ${ANDROID_HOME}/cmdline-tools/latest/bin/sdkmanager \
      "platform-tools" \
      "platforms;android-35" \
      "build-tools;35.0.0"

ENV PATH="${PATH}:${ANDROID_HOME}/cmdline-tools/latest/bin:${ANDROID_HOME}/platform-tools"
