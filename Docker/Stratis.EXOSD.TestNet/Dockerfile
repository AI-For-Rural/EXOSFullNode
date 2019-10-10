FROM microsoft/dotnet:2.2-sdk

RUN git clone https://github.com/exoeconomy/EXOSFullNode.git \
    && cd /EXOSFullNode/src/Stratis.EXOSD \
	&& dotnet build
	
VOLUME /root/.exosnode

WORKDIR /EXOSFullNode/src/Stratis.EXOSD

COPY exos.conf.docker /root/.exosnode/exos/EXOSMain/exos.conf

EXPOSE 14562 14561 39121 39621

CMD ["dotnet", "run","-testnet"]