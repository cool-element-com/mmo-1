FROM ubuntu:22.04

# Avoid prompts from apt
ENV DEBIAN_FRONTEND=noninteractive

# Install dependencies
RUN apt-get update && apt-get install -y \
    wget \
    apt-transport-https \
    software-properties-common \
    git \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install Microsoft package repository
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb

# Install .NET SDK
RUN apt-get update && apt-get install -y dotnet-sdk-8.0 \
    && rm -rf /var/lib/apt/lists/*

# Set up working directory
WORKDIR /app

# Clone the repository and checkout the specific branch
RUN git clone https://github.com/cool-element-com/mmo-1.git \
    && cd mmo-1 \
    && git checkout manus-iteration-1

# Set the working directory to the project folder
WORKDIR /app/mmo-1/source/SpacetimeDBClient/Web/PokerClient

# Install SpacetimeDB CLI (commented out as it requires specific version matching)
# Uncomment and modify this section when you have the correct CLI version
# RUN curl -L https://github.com/clockworklabs/spacetimedb/releases/download/v0.x.x/spacetime-v0.x.x-x86_64-unknown-linux-gnu.tar.gz -o spacetime.tar.gz \
#    && tar -xzf spacetime.tar.gz \
#    && mv spacetime /usr/local/bin/ \
#    && rm spacetime.tar.gz

# Expose port if needed
# EXPOSE 5000

# Set the entry point to bash so the container stays running
CMD ["/bin/bash"]
