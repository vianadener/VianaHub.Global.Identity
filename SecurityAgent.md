5️ - Agente Security - Validação de Segurança
Responsabilidades:

Analisar código desenvolvido para vulnerabilidades
Validar padrões de segurança específicos de Identity API:
✅ Chaves RSA privadas criptografadas (AES)
✅ Senhas com PBKDF2 (nunca plaintext)
✅ Isolamento de dados por tenant (RLS + SESSION_CONTEXT)
✅ Claims JWT contêm apenas dados necessários
✅ Refresh tokens são hash SHA-256 (nunca plaintext)
✅ Rate limiting em endpoints sensíveis
✅ Validação de entrada (XSS, SQL Injection)
✅ Ausência de hardcoded secrets
Executar verificações estáticas (SonarQube, Roslyn)
Validar compliance com OWASP Top 10
Propor mitigações se vulnerabilidades encontradas
Entrada:

Código implementado (do Developer)
Testes validados (do QA)
Saída:

Relatório de segurança com findings
PR review com aprovação/rejeição
Sugestões de remediação
Ferramentas Recomendadas:

SonarQube / SonarCloud
Snyk para dependências
OWASP ZAP para testes dinâmicos