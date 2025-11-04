# SteeringBehavioursDemo

Steering Behaviours implementados:

* Seek/Flee: personagens se aproximam ou afastam do alvo;
* Arrival/Departure: personagens se aproximam ou afastam do alvo com desaceleração ao chegar e aceleração ao sair variáveis;
* Leader: personagens seguem um líder, mantendo certa distância e evitando ficar na frente do campo de visão do líder, com o líder utilizando o Arrival behaviour;
* Separation: personagens buscam manter uma certa distância uns dos outros e param de se mover quando estabilizam acima da distância mínima;
* Cohesion: personagens próximos buscam se encontrar num ponto em comum;
* Multiple (BOIDs): combinação de cinco steering behaviours: Alignment (personagens buscam manter a mesma velocidade), Separation, Cohesion, Leader e Arrival;

Controles para utilizar o programa:

* Teclas WASD: controle de movimento de câmera;
* Botão esquerdo do mouse pressionado + Movimento do mouse: controle de rotação da câmera;
* Clique/segurar botão direito do mouse + Movimento do mouse: movimentar alvo dos personagens;
* Tecla Space: ativa/desativa menu de steering behaviours;
* Menu de steering behaviours: clique com o botão direito do mouse sobre o botão de steering behaviour desejado para ativá-lo;

Estrtura do código:

* AI_Manager.cs: Todo código relacionado aos steering behaviours e controle de posição e velocidade aplicados aos personagens se encontra nesse script;
* TargetInputController.cs: seta a posição do alvo para os personagens no mundo baseado no input do usuário. 
