FROM microsoft/dotnet:2.1-sdk

RUN git clone https://github.com/FluidChains/FluidChainsFullNode.git \
    && cd /FluidChainsFullNode/src/Stratis.FluidD \
	&& dotnet build
	
VOLUME /root/.stratisnode

WORKDIR /FluidChainsFullNode/src/Stratis.FluidD

COPY fluid.conf.docker /root/.stratisnode/fluid/FluidMain/fluid.conf

EXPOSE 39391 39390 37222 38825

CMD ["dotnet", "run"]
