FROM microsoft/dotnet:2.1-sdk

 RUN git clone https://github.com/exoeconomy/CivXFullNode.git \
  && cd /CivXFullNode/src/Stratis.StratisD \
  && dotnet build

VOLUME /root/.stratisnode

COPY exos.conf.docker /root/.stratisnode/exos/EXOSMain/exos.conf

EXPOSE 4561 4562 37220

WORKDIR /CivXFullNode/src/Stratis.StratisD

CMD ["dotnet", "run"]